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
            public Type AppServiceType { get; set; }
        }
        public static List<ServiceTypeItem> ControlTypes { get; set; }
        static SysUtil()
        {
            ControlTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(ApplicationRemoteService).IsAssignableFrom(type) && type.IsSubclassOf(typeof(ApplicationRemoteService)) && type.IsClass)
                .Select(type => new ServiceTypeItem()
                {
                    ServiceKey = type.GetServiceKey() ?? throw new Exception(type.Name + ":The serviceKey cannot be empty!"),
                    AppServiceType = type
                })
                .ToList();
        }
    }
}
