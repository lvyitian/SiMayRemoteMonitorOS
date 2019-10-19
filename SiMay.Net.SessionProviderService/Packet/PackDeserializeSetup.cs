using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.Package
{
    public class PackDeserializeSetup
    {
        private int _index = 0;
        private List<byte> _arr = new List<byte>();
        public Encoding Encoding { get; set; } = Encoding.Unicode;
        internal PackDeserializeSetup(byte[] data, object @object)
        {
            this._arr.AddRange(data);
            this.ActionDeserialize(@object);
            this._arr.Clear();
        }
        private void ActionDeserialize(object @object)
        {
            var properties = @object.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var type = property.PropertyType;
                var value = property.GetValue(@object, null);
                if (type.Equals(typeof(Boolean)))
                    property.SetValue(@object, this.ReadBoolean());
                else if (type.Equals(typeof(Boolean[])))
                    property.SetValue(@object, this.ReadArrayBoolean());
                else if (type.Equals(typeof(Byte)))
                    property.SetValue(@object, this.ReadByte());
                else if (type.Equals(typeof(Byte[])))
                    property.SetValue(@object, this.ReadBytes());
                else if (type.Equals(typeof(Int16)))
                    property.SetValue(@object, this.ReadInt16());
                else if (type.Equals(typeof(Int16[])))
                    property.SetValue(@object, this.ReadArrayInt16());
                else if (type.Equals(typeof(Int32)))
                    property.SetValue(@object, this.ReadInt32());
                else if (type.Equals(typeof(Int32[])))
                    property.SetValue(@object, this.ReadArrayInt32());
                else if (type.Equals(typeof(Int64)))
                    property.SetValue(@object, this.ReadInt64());
                else if (type.Equals(typeof(Int64[])))
                    property.SetValue(@object, this.ReadArrayInt64());
                else if (type.Equals(typeof(String)))
                    property.SetValue(@object, this.ReadString());
                else if (type.Equals(typeof(String[])))
                    property.SetValue(@object, this.ReadArrayString());
                else if (type.IsArray)
                    property.SetValue(@object, this.ReadArray(property.PropertyType));
                else
                    this.ActionDeserialize(value);
            }
        }
        private bool[] ReadArrayBoolean()
        {
            int len = this.ReadInt32();
            bool[] Arraybool = new bool[len];
            if (len > 0)
            {
                var bytes = this.Read(len);
                for (int i = 0; i < len; i++)
                    Arraybool[i] = bytes[i] == 1 ? true : false;
            }
            return Arraybool;
        }
        private short[] ReadArrayInt16()
        {
            int len = this.ReadInt32();
            short[] ArrayInt16 = new short[len];
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                    ArrayInt16[i] = this.ReadInt16();
            }
            return ArrayInt16;
        }
        private int[] ReadArrayInt32()
        {
            int len = this.ReadInt32();
            int[] ArrayInt32 = new int[len];
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                    ArrayInt32[i] = this.ReadInt32();
            }
            return ArrayInt32;
        }
        private long[] ReadArrayInt64()
        {
            int len = this.ReadInt32();
            long[] ArrayInt64 = new long[len];
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                    ArrayInt64[i] = this.ReadInt64();
            }
            return ArrayInt64;
        }
        private string[] ReadArrayString()
        {
            int len = this.ReadInt32();
            string[] ArrayString = new string[len];
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                    ArrayString[i] = this.ReadString();
            }
            return ArrayString;
        }

        private Array ReadArray(Type type)
        {
            int len = this.ReadInt32();
            object array = null;
            array = type.InvokeMember("Set", BindingFlags.CreateInstance, null, array, new object[] { len });
            for (int i = 0; i < len; i++)
            {
                var @object = Activator.CreateInstance(type.GetElementType());
                this.ActionDeserialize(@object);
                type.GetMethod("SetValue", new Type[2] { typeof(object), typeof(int) }).Invoke(array, new object[] { @object, i });
            }
            return (Array)array;
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
        private string ReadString()
        {
            int len = this.ReadInt32();
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
            if (len > 0)
                return this.Read(len);
            else
                return new byte[0];
        }
        private byte[] Read(int lenght)
        {
            if ((_index + lenght) > this._arr.Count)
                throw new ArgumentOutOfRangeException();

            byte[] bytes = this._arr.GetRange(_index, lenght).ToArray();
            _index += lenght;
            return bytes;
        }
    }
}

