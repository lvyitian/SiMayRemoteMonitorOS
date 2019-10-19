using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum o)
        {
            Type enumType = o.GetType();
            string name = Enum.GetName(enumType, o);
            DescriptionAttribute customAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(enumType.GetField(name), typeof(DescriptionAttribute));
            if (customAttribute != null)
            {
                return customAttribute.Description;
            }
            return name;
        }
    }
}
