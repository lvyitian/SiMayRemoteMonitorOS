using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Basic;

namespace SiMay.RemoteControlsCore
{
    public static class AppTypeExtension
    {
        public static Type GetAppAdapterHandlerType(this Type type)
        {
            var attr = type.GetCustomAttribute<ApplicationAttribute>(true);
            return attr.AppHandlerAdapterType;
        }
        public static string GetApplicationKey(this Type type)
        {
            var attr = type.GetCustomAttribute<ApplicationAttribute>(true);
            return attr.ApplicationKey;
        }

        public static int GetRank(this Type type)
        {
            var attr = type.GetCustomAttribute<ApplicationAttribute>(true);
            return attr.Rank;
        }
    }
}
