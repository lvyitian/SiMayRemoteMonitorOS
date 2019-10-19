using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public class SysUtil
    {
        public class ApplicationItem
        {
            public int Rank { get; set; }
            public string AppKey { get; set; }
            public Type Type { get; set; }
        }
        public static IEnumerable<ApplicationItem> ApplicationTypes { get; private set; }
        static SysUtil()
        {
            List<ApplicationItem> applicationTypes = new List<ApplicationItem>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IApplication).IsAssignableFrom(type) && type.IsClass)
                    {
                        if (type.GetCustomAttribute<DisableAttribute>() != null)
                            continue;
                        var context = new ApplicationItem()
                        {
                            Rank = type.GetRank(),
                            AppKey = type.GetAppKey() ?? throw new Exception(type.Name + ":The AppKey cannot be empty!"),
                            Type = type
                        };
                        applicationTypes.Add(context);
                    }
                } 
            }
            ApplicationTypes = applicationTypes;
        }
    }
}
