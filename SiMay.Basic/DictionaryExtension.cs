using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public static class DictionaryExtension
    {
        public static void AddRange(this IDictionary<string, object> source, IDictionary<string, object> target)
        {
            if (source == null || target.IsNullOrEmpty()) return;
            foreach (var item in target)
            {
                if (!source.Keys.Contains(item.Key, StringComparer.OrdinalIgnoreCase))
                {
                    source.Add(item);
                }
            }
        }

        public static T TryGetValue<T>(this IDictionary<string, T> source, string key) where T : class
        {
            T value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public static object GetValue(this IDictionary<string, object> source, string key)
        {
            object value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public static T GetValue<T>(this IDictionary<string, T> source, string key)
        {
            T value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return default(T);
        }

        public static V GetValue<K, V>(this IDictionary<K, V> source, K key)
        {
            V value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return default(V);
        }

        public static IDictionary<K, V> Clone<K, V>(this IDictionary<K, V> source)
        {
            var type = source.GetType();
            var instance = Activator.CreateInstance(type) as IDictionary<K, V>;

            if (instance != null)
            {
                foreach (var key in source.Keys)
                {
                    instance.Add(key, source[key]);
                }
            }
            return instance;
        }
    }
}
