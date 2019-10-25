using SiMay.RemoteMonitor.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Basic;

namespace SiMay.RemoteMonitor.Extensions
{
    public static class AppTypeExtension
    {
        public static string GetApplicationName(this Type type)
        {
            var attr = type.GetCustomAttribute<ApplicationNameAttribute>(true);
            return attr.Name;
        }
        public static string GetIconResourceName(this Type type)
        {
            var attr = type.GetCustomAttribute<AppResourceNameAttribute>(true);
            return attr.Name;
        }

        public static bool OnTools(this Type type)
        {
            var attr = type.GetCustomAttribute<OnToolsAttribute>(true);
            return attr != null;
        }
    }
}
