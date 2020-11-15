using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiMay.Service.Core
{
    public class SysUtil
    {
        public class ServiceTypeItem
        {
            public string RemoteServiceKey { get; set; }
            public Type RemoteServiceType { get; set; }
        }
        public static List<ServiceTypeItem> RemoteServiceTypes { get; set; }
        static SysUtil()
        {
            RemoteServiceTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(ApplicationRemoteService).IsAssignableFrom(type) && type.IsSubclassOf(typeof(ApplicationRemoteService)) && type.IsClass)
                .Select(type => new ServiceTypeItem()
                {
                    RemoteServiceKey = type.GetApplicationKey() ?? throw new Exception(type.Name + ":the serviceKey cannot be empty!"),
                    RemoteServiceType = type
                })
                .ToList();
        }
    }
}
