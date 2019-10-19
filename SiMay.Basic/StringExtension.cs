using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public static class StringExtension
    {
        public static string Substring(this string str, int length, string endOf)
        {
            if (!string.IsNullOrEmpty(str) && ((length > 0) && (length < str.Length)))
            {
                return (str.Substring(0, length) + endOf);
            }
            return str;
        }

        public static string FormatTo(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string CharAt(this string s, int index)
        {
            if ((index >= s.Length) || (index < 0))
                return "";
            return s.Substring(index, 1);
        }

        public static T ToEnum<T>(this string str) where T : struct
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            return (T)Enum.Parse(typeof(T), str);
        }

        public static T ToEnum<T>(this string str, bool ignoreCase) where T : struct
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            return (T)Enum.Parse(typeof(T), str, ignoreCase);
        }

        public static DateTime ToDateTime(this string str, string format)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            DateTime datetime;
            if (DateTime.TryParseExact(str, format, null, System.Globalization.DateTimeStyles.None, out datetime))
            {
                return datetime;
            }
            else
            {
                throw new FormatException("str");
            }

        }
    }
}
