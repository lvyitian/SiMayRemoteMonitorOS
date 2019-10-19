using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SiMay.Package
{
    public class PackSerializeSetup
    {
        public Encoding Encoding { get; set; } = Encoding.Unicode;

        List<byte> _arr = new List<byte>();
        internal PackSerializeSetup(object @object)
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
                    this.WriteArrayInt16((Int16[])value);
                else if (type.Equals(typeof(Int32)))
                    this.WriteInt32((Int32)value);
                else if (type.Equals(typeof(Int32[])))
                    this.WriteArrayInt32((Int32[])value);
                else if (type.Equals(typeof(Int64)))
                    this.WriteInt64((Int64)value);
                else if (type.Equals(typeof(Int64[])))
                    this.WriteArrayInt64((Int64[])value);
                else if (type.Equals(typeof(String)))
                    this.WriteString((String)value);
                else if (type.Equals(typeof(String[])))
                    WriteArrayString((String[])value);
                else if (type.IsArray)
                    this.WriteArray((Array)value);
                else
                    this.ActionSerialize(value);
            }
        }


        public byte[] ToArray()
        {
            byte[] buffer = this._arr.ToArray();
            this._arr.Clear();

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

        private void WriteArrayInt16(Int16[] val)
        {
            this.WriteInt32(val.Length);
            foreach (Int16 b in val)
                this.WriteInt16(b);
        }

        private void WriteArrayInt32(Int32[] val)
        {
            this.WriteInt32(val.Length);
            foreach (Int32 b in val)
                this.WriteInt32(b);
        }

        private void WriteArrayInt64(Int64[] val)
        {
            this.WriteInt32(val.Length);
            foreach (Int32 b in val)
                this.WriteInt64(b);
        }

        private void WriteArrayString(String[] val)
        {
            String[] strArray = val as String[];
            this.WriteInt32(strArray.Length);
            foreach (var str in strArray)
                this.WriteString(str);
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

        private void WriteSingle(Single f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            this.Write(bytes);
        }

        private void WriteDouble(double d)
        {
            byte[] bytes = BitConverter.GetBytes(d);
            this.Write(bytes);
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
            this._arr.AddRange(bytes);
        }
        private void WriteByte(byte b)
        {
            this._arr.Add(b);
        }
    }
}
