using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Basic;
using SiMay.ServiceCore.Attributes;

namespace SiMay.ServiceCore
{
    public static class ServiceTypeExtension
    {
        public static string GetServiceKey(this Type type)
        {
            var attr = type.GetCustomAttribute<ServiceKeyAttribute>(true);
            return attr == null ? null : (attr as ServiceKeyAttribute).Key;
        }
    }
}
