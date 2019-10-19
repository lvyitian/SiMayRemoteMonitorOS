using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public static class CollectionExtension
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count == 0;
        }
    }
}
