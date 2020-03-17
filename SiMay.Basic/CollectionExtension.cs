using System;
using System.Collections;
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

        public static T FristOrDefault<T>(this ICollection<T> source, Func<T, bool> func)
        {
            foreach (var item in source)
            {
                if (func(item))
                    return item;
            }
            return default;
        }

        public static T FristOrDefault<T>(this ICollection source)
        {
            foreach (var item in source)
            {
                return (T)item;
            }
            return default;
        }

        public static T FristOrDefault<T>(this ICollection source, Func<T, bool> func)
        {
            foreach (var item in source)
            {
                if (func((T)item))
                    return (T)item;
            }
            return default;
        }

        public static IEnumerable<Target> Select<T, Target>(this ICollection source, Func<T, Target> func)
        {
            var results = new List<Target>();
            foreach (var item in source)
                results.Add(func((T)item));

            return results;
        }
    }
}
