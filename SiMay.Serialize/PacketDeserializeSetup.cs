using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.Serialize
{
    public class PacketDeserializeSetup
    {
        private int _index = 0;
        private List<byte> _bytesArr = new List<byte>();
        public Encoding Encoding { get; set; } = Encoding.Unicode;
        internal PacketDeserializeSetup(byte[] data, object @object)
        {
            this._bytesArr.AddRange(data);
            this.ActionDeserialize(@object);
            this._bytesArr.Clear();
        }
        private void ActionDeserialize(object @object)
        {
            var properties = @object.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var type = property.PropertyType;
                if (type.Equals(typeof(Boolean)))
                    property.SetValue(@object, this.ReadBoolean(), null);
                else if (type.Equals(typeof(Boolean[])))
                    property.SetValue(@object, this.ReadArray(ReadBoolean), null);
                else if (type.Equals(typeof(Byte)))
                    property.SetValue(@object, this.ReadByte(), null);
                else if (type.Equals(typeof(Byte[])))
                    property.SetValue(@object, this.ReadBytes(), null);
                else if (type.Equals(typeof(Int16)))
                    property.SetValue(@object, this.ReadInt16(), null);
                else if (type.Equals(typeof(Int16[])))
                    property.SetValue(@object, this.ReadArray(ReadInt16), null);
                else if (type.Equals(typeof(Int32)))
                    property.SetValue(@object, this.ReadInt32(), null);
                else if (type.Equals(typeof(Int32[])))
                    property.SetValue(@object, this.ReadArray(ReadInt32), null);
                else if (type.Equals(typeof(Int64)))
                    property.SetValue(@object, this.ReadInt64(), null);
                else if (type.Equals(typeof(Int64[])))
                    property.SetValue(@object, this.ReadArray(ReadInt64), null);
                else if (type.Equals(typeof(String)))
                    property.SetValue(@object, this.ReadString(), null);
                else if (type.Equals(typeof(String[])))
                    property.SetValue(@object, this.ReadArray(ReadString), null);
                else if (type.Equals(typeof(Single)))
                    property.SetValue(@object, this.ReadFloat(), null);
                else if (type.Equals(typeof(Single[])))
                    property.SetValue(@object, this.ReadArray(ReadFloat), null);
                else if (type.Equals(typeof(Double)))
                    property.SetValue(@object, this.ReadDouble(), null);
                else if (type.Equals(typeof(Double[])))
                    property.SetValue(@object, this.ReadArray(ReadDouble), null);
                else if (type.Equals(typeof(DateTime)))
                    property.SetValue(@object, this.ReadDateTime(), null);
                else if (type.BaseType.Equals(typeof(Enum)))
                    property.SetValue(@object, this.ReadEnum(property.PropertyType), null);
                else if (type.IsArray)
                    property.SetValue(@object, this.ReadArray(property.PropertyType), null);
                else
                {
                    if (this.ReadInt32() == 1)
                    {
                        var instance = Activator.CreateInstance(property.PropertyType);
                        this.ActionDeserialize(instance);
                        property.SetValue(@object, instance, null);
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
        private Array ReadArray(Type type)
        {
            int len = this.ReadInt32();
            if (len == -1)
                return null;
            var elementType = type.GetElementType();
            var array = Array.CreateInstance(elementType, len);
            for (int i = 0; i < len; i++)
            {
                var @object = Activator.CreateInstance(elementType);
                this.ActionDeserialize(@object);
                array.SetValue(@object, i);
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
                return new DateTime();
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
            _index += lenght;
            return bytes;
        }
    }
}

