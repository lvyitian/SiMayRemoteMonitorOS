using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Basic;
using SiMay.Core;

namespace SiMay.Service.Core
{
    public static class ServiceTypeExtension
    {
        public static string GetApplicationKey(this Type type)
        {
            var attr = type.GetCustomAttribute<ApplicationServiceKeyAttribute>(true);
            return attr == null ? null : (attr as ApplicationServiceKeyAttribute).Key;
        }
    }
}
