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
            public string ApplicationKey { get; set; }
            public Type Type { get; set; }
        }
        public static IReadOnlyList<ApplicationItem> ApplicationTypes { get; private set; }
        static SysUtil()
        {
            ApplicationTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IApplication).IsAssignableFrom(t) && t.IsClass)
                .Where(t => t.GetCustomAttribute<DisableAttribute>() == null)
                .Select(t => new ApplicationItem()
                {
                    Rank = t.GetRank(),
                    ApplicationKey = t.GetAppKey() ?? throw new Exception(t.Name + ":The AppKey cannot be empty!"),
                    Type = t
                })
                .ToList();
        }
    }
}
