using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Basic
{
    public static class ArrayHelper
    {
        public static T[] Copy<T>(this T[] source, int offset, int length)
        {
            if (offset == 0 && length == source.Length)
                return source;

            var nArray = new T[length];
            Array.Copy(source, offset, nArray, 0, length);
            return nArray;
        }
    }
}
