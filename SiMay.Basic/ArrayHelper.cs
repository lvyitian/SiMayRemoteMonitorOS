using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Basic
{
    public static class ArrayHelper
    {
        public static T[] Copy<T>(this T[] source, int offset, int lenght)
        {
            var nArray = new T[lenght];
            Array.Copy(source, offset, nArray, 0, lenght);
            return nArray;
        }
    }
}
