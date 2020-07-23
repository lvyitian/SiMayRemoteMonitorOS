using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiMay.ServiceCore
{
    public class SysUtil
    {
        public class ServiceTypeItem
        {
            public string ServiceKey { get; set; }
            public Type ApplicationServiceType { get; set; }
        }
        public static List<ServiceTypeItem> ServiceTypes { get; set; }
        static SysUtil()
        {
            ServiceTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(ApplicationRemoteService).IsAssignableFrom(type) && type.IsSubclassOf(typeof(ApplicationRemoteService)) && type.IsClass)
                .Select(type => new ServiceTypeItem()
                {
                    ServiceKey = type.GetApplicationKey() ?? throw new Exception(type.Name + ":the serviceKey cannot be empty!"),
                    ApplicationServiceType = type
                })
                .ToList();
        }
    }
}
