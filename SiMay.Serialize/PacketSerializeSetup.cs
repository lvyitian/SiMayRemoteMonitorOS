using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SiMay.Serialize
{
    public class PacketSerializeSetup
    {
        public Encoding Encoding { get; set; } = Encoding.Unicode;

        List<byte> _bytesArr = new List<byte>();
        internal PacketSerializeSetup(object @object)
        {
            this.ActionSerialize(@object);
        }
        private void ActionSerialize(object @object)
        {
            var properties = @object.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                var type = property.PropertyType;
                var value = property.GetValue(@object, null);

                if (value == null)
                {
                    this.WriteInt32(-1);
                    continue;
                }

                if (type.Equals(typeof(Boolean)))
                    this.WriteByte((Boolean)value ? (Byte)1 : (Byte)0);
                else if (type.Equals(typeof(Boolean[])))
                    this.WriteArrayBool((Boolean[])value);
                else if (type.Equals(typeof(Byte)))
                    this.WriteByte((Byte)value);
                else if (type.Equals(typeof(Byte[])))
                    this.WriteArrayByte((Byte[])value);
                else if (type.Equals(typeof(Int16)))
                    this.WriteInt16((Int16)value);
                else if (type.Equals(typeof(Int16[])))
                    this.WriteArray((Int16[])value, WriteInt16);
                else if (type.Equals(typeof(Int32)))
                    this.WriteInt32((Int32)value);
                else if (type.Equals(typeof(Int32[])))
                    this.WriteArray((Int32[])value, WriteInt32);
                else if (type.Equals(typeof(Int64)))
                    this.WriteInt64((Int64)value);
                else if (type.Equals(typeof(Int64[])))
                    this.WriteArray((Int64[])value, WriteInt64);
                else if (type.Equals(typeof(String)))
                    this.WriteString((String)value);
                else if (type.Equals(typeof(String[])))
                    this.WriteArray((String[])value, WriteString);
                else if (type.Equals(typeof(Single)))
                    this.WriteFloat((Single)value);
                else if (type.Equals(typeof(Single[])))
                    this.WriteArray((Single[])value, WriteFloat);
                else if (type.Equals(typeof(Double)))
                    this.WriteDouble((Double)value);
                else if (type.Equals(typeof(Double[])))
                    this.WriteArray((Double[])value, WriteDouble);
                else if (type.Equals(typeof(DateTime)))
                    this.WriteDateTime((DateTime)value);
                else if (type.BaseType.Equals(typeof(Enum)))
                    this.WriteEnum(value);
                else if (type.IsArray)
                {
                    if (type.GetElementType().IsValueType)
                        throw new Exception("not supported this value array!");

                    this.WriteArray((Array)value);
                }
                else if (type.IsClass)
                {
                    this.WriteInt32(1);//是否为空标志位
                    this.ActionSerialize(value);
                }
                else
                    throw new Exception("not supported this type:" + type.FullName);
            }
        }
        public byte[] ToArray()
        {
            byte[] buffer = this._bytesArr.ToArray();
            this._bytesArr.Clear();

            return buffer;
        }

        private void WriteArrayBool(bool[] val)
        {
            this.WriteInt32(val.Length);
            foreach (bool b in val)
                this.WriteByte((byte)(b ? 1 : 0));
        }

        private void WriteArrayByte(byte[] val)
        {
            this.WriteInt32(val.Length);
            this.Write(val);
        }

        private void WriteArray<T>(T[] val, Action<T> func)
        {
            this.WriteInt32(val.Length);
            foreach (T item in val)
                func(item);
        }
        private void WriteArray(Array val)
        {
            this.WriteInt32(val.Length);
            foreach (var item in val)
                this.ActionSerialize(item);
        }
        private void WriteInt16(short s)
        {
            byte[] bytes = BitConverter.GetBytes(s);
            this.Write(bytes);
        }

        private void WriteInt32(int i)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            this.Write(bytes);
        }

        private void WriteInt64(long l)
        {
            byte[] bytes = BitConverter.GetBytes(l);
            this.Write(bytes);
        }

        private void WriteFloat(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            this.Write(bytes);
        }

        private void WriteDouble(double d)
        {
            byte[] bytes = BitConverter.GetBytes(d);
            this.Write(bytes);
        }
        private void WriteDateTime(DateTime time)
        {
            string str = time.ToString("yyyy");
            if (str == "0001")
                this.WriteInt64(0);
            else
                this.WriteInt64(time.ToFileTime());
        }
        private void WriteEnum(object @enum)
        {
            var enumValType = Enum.GetUnderlyingType(@enum.GetType());
            if (enumValType.Equals(typeof(byte)))
                this.WriteByte((byte)@enum);
            else if (enumValType.Equals(typeof(short)))
                this.WriteInt16((short)@enum);
            else if (enumValType.Equals(typeof(int)))
                this.WriteInt32((int)@enum);
            else
                this.WriteInt64((long)@enum);
        }
        private void WriteString(string str)
        {
            if (str.Length == 0)
            {
                this.WriteInt32(0);
                return;
            }
            byte[] bytes = Encoding.GetBytes(str);
            this.WriteInt32(bytes.Length);
            this.Write(bytes);
        }

        private void Write(byte[] bytes)
        {
            this._bytesArr.AddRange(bytes);
        }
        private void WriteByte(byte b)
        {
            this._bytesArr.Add(b);
        }
    }
}
