using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public static class ObjectHelper
    {
        public static T ConvertTo<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
