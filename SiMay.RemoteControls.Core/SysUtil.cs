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
            /// <summary>
            /// 应用显示名称
            /// </summary>
            public string ApplicationName { get; set; }

            /// <summary>
            /// 应用类型
            /// </summary>
            public Type Type { get; set; }
        }
        public static IReadOnlyList<ApplicationItem> ApplicationTypes { get; private set; }
        static SysUtil()
        {
            ApplicationTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IApplication).IsAssignableFrom(t) && t.IsClass && t.GetCustomAttribute<DisableAttribute>() == null)
                .Select(type => new ApplicationItem()
                {
                    ApplicationName = type.Name,
                    Type = type
                })
                .ToList();
        }
    }
}
