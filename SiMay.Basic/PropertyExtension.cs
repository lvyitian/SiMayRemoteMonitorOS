using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.Basic
{
    public static class PropertyExtension
    {
        public static T GetCustomAttribute<T>(this PropertyInfo property, bool inherit) where T : Attribute
        {
            return (T)property.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static string GetDisplayName(this PropertyInfo property)
        {
            string result = property.Name;
            var attr = property.GetCustomAttribute<DisplayNameAttribute>(true);
            if (attr != null)
            {
                result = attr.DisplayName;
            }
            return result;
        }

        public static string GetDescription(this PropertyInfo property)
        {
            string result = string.Empty;
            var attr = property.GetCustomAttribute<DescriptionAttribute>(true);
            if (attr != null)
            {
                result = attr.Description;
            }
            return result;
        }
    }
}
