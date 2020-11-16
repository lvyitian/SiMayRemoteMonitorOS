using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SiMay.Serialize.Standard
{
    public class PacketDeserializeSetup
    {
        private volatile int _index = 0;
        private List<byte> _bytesArr = new List<byte>();
        public Encoding Encoding { get; set; } = Encoding.Unicode;

        internal PacketDeserializeSetup(byte[] data, object instance)
        {
            IMemberAccessor memberAccessor = DynamicMethodMemberAccessor.FindClassAccessor(instance.GetType());
            this._bytesArr.AddRange(data);
            this.StartDeserialize(instance, memberAccessor);
            this._bytesArr.Clear();
        }
        private void StartDeserialize(object instance, IMemberAccessor memberAccessor)
        {
            var properties = instance.GetType().GetProperties();
            foreach (PropertyInfo property in properties.OrderBy(c => c.Name))
            {
                var type = property.PropertyType;
                if (type.Equals(typeof(Boolean)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadBoolean());
                else if (type.Equals(typeof(Boolean[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadBoolean));
                else if (type.Equals(typeof(Byte)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadByte());
                else if (type.Equals(typeof(Byte[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadBytes());
                else if (type.Equals(typeof(Int16)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadInt16());
                else if (type.Equals(typeof(Int16[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadInt16));
                else if (type.Equals(typeof(Int32)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadInt32());
                else if (type.Equals(typeof(Int32[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadInt32));
                else if (type.Equals(typeof(Int64)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadInt64());
                else if (type.Equals(typeof(Int64[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadInt64));
                else if (type.Equals(typeof(String)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadString());
                else if (type.Equals(typeof(String[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadString));
                else if (type.Equals(typeof(Single)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadFloat());
                else if (type.Equals(typeof(Single[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadFloat));
                else if (type.Equals(typeof(Double)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadDouble());
                else if (type.Equals(typeof(Double[])))
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(ReadDouble));
                else if (type.Equals(typeof(DateTime)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadDateTime());
                else if (type.BaseType.Equals(typeof(Enum)))
                    memberAccessor.SetValue(instance, property.Name, this.ReadEnum(property.PropertyType));
                else if (type.IsArray)
                {
                    if (type.GetElementType().IsValueType)
                        throw new Exception("not supported this value array!");

                    var elementType = type.GetElementType();
                    var arrayMemberAccessor = memberAccessor.Type.Equals(elementType) ? memberAccessor : DynamicMethodMemberAccessor.FindClassAccessor(elementType);
                    memberAccessor.SetValue(instance, property.Name, this.ReadArray(property.PropertyType, arrayMemberAccessor));
                }
                else
                {
                    if (this.ReadInt32() == 1)
                    {
                        var childerInstanceMemberAccessor = memberAccessor.Type.Equals(property.PropertyType) ? memberAccessor : DynamicMethodMemberAccessor.FindClassAccessor(property.PropertyType);
                        var childerInstance = Activator.CreateInstance(property.PropertyType);
                        this.StartDeserialize(childerInstance, childerInstanceMemberAccessor);
                        childerInstanceMemberAccessor.SetValue(instance, property.Name, childerInstance);
                    }
                }
            }
        }
        private Array ReadArray<T>(Func<T> func)
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;

            T[] array = new T[len];
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                    array[i] = func();
            }
            return array;
        }
        private Array ReadArray(Type type, IMemberAccessor memberAccessor)
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
            var elementType = type.GetElementType();
            var array = Array.CreateInstance(elementType, len);
            for (int i = 0; i < len; i++)
            {
                var instance = Activator.CreateInstance(elementType);
                this.StartDeserialize(instance, memberAccessor);
                array.SetValue(instance, i);
            }
            return array;
        }
        private byte ReadByte()
        {
            return this.Read(1)[0];
        }
        private bool ReadBoolean()
        {
            byte[] buffer = this.Read(1);
            return BitConverter.ToBoolean(buffer, 0);
        }
        private short ReadInt16()
        {
            byte[] buffer = this.Read(sizeof(Int16));
            return BitConverter.ToInt16(buffer, 0);
        }
        private int ReadInt32()
        {
            byte[] buffer = this.Read(sizeof(Int32));
            return BitConverter.ToInt32(buffer, 0);
        }
        private long ReadInt64()
        {
            byte[] buffer = this.Read(sizeof(Int64));
            return BitConverter.ToInt64(buffer, 0);
        }

        private float ReadFloat()
        {
            byte[] buffer = this.Read(sizeof(Single));
            return BitConverter.ToSingle(buffer, 0);
        }

        private double ReadDouble()
        {
            byte[] buffer = this.Read(sizeof(Double));
            return BitConverter.ToDouble(buffer, 0);
        }

        private DateTime ReadDateTime()
        {
            long fileTime = this.ReadInt64();
            if (fileTime == 0)
                return default;
            else
                return DateTime.FromFileTime(fileTime);
        }
        private object ReadEnum(Type type)
        {
            var enumValType = Enum.GetUnderlyingType(type);
            if (enumValType.Equals(typeof(byte)))
                return Enum.ToObject(type, this.ReadByte());
            else if (enumValType.Equals(typeof(short)))
                return Enum.ToObject(type, this.ReadInt16());
            else if (enumValType.Equals(typeof(int)))
                return Enum.ToObject(type, this.ReadInt32());
            else
                return Enum.ToObject(type, this.ReadInt64());
        }
        private string ReadString()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;

            string str = string.Empty;
            if (len > 0)
            {
                byte[] strBytes = this.Read(len);
                str = Encoding.GetString(strBytes);
            }
            return str;
        }
        private byte[] ReadBytes()
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;

            if (len > 0)
                return this.Read(len);
            else
                return new byte[0];
        }
        private byte[] Read(int lenght)
        {
            if ((_index + lenght) > this._bytesArr.Count)
                throw new ArgumentOutOfRangeException();

            byte[] bytes = this._bytesArr.GetRange(_index, lenght).ToArray();
            Interlocked.Add(ref _index, lenght);
            return bytes;
        }
    }
}

