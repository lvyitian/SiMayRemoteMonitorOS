using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    /// <summary>
    /// 为<see cref="IEnumerable{T}"/>提供常用的扩展方法
    /// </summary>
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (!enumerable.IsNullOrEmpty())
            {
                foreach (var item in enumerable)
                {
                    action(item);
                }
            }
        }

        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            if (!source.IsNullOrEmpty())
            {
                return source.Distinct(new CommonEqualityComparer<T, V>(keySelector));
            }
            return null;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }

    public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private Func<T, V> keySelector;

        public CommonEqualityComparer(Func<T, V> keySelector)
        {
            this.keySelector = keySelector;
        }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<V>.Default.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return EqualityComparer<V>.Default.GetHashCode(keySelector(obj));
        }
    }
}
