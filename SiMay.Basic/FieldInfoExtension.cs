using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.Basic
{
    public static class FieldInfoExtension
    {
        public static T GetCustomAttribute<T>(this FieldInfo property, bool inherit) where T : Attribute
        {
            return (T)property.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static string GetDisplayName(this FieldInfo property)
        {
            string result = property.Name;
            var attr = property.GetCustomAttribute<DisplayNameAttribute>(true);
            if (attr != null)
            {
                result = attr.DisplayName;
            }
            return result;
        }

        public static string GetDescription(this FieldInfo property)
        {
            string result = property.Name;
            var attr = property.GetCustomAttribute<DescriptionAttribute>(true);
            if (attr != null)
            {
                result = attr.Description;
            }
            return result;
        }
    }
}
