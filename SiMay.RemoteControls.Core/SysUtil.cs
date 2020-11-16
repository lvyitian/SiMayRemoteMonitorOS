using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace SiMay.RemoteControls.Core
{
    public static class SysUtil
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
            public Type ApplicationType { get; set; }
        }

        public static IList<ApplicationItem> ApplicationTypes { get; } = new List<ApplicationItem>();

        public static IList<ApplicationItem> ApplicationRegister<T>(this IList<ApplicationItem> applications)
            where T : IApplication, new()
        {
            var type = typeof(T);
            applications.Add(new ApplicationItem()
            {
                ApplicationName = type.Name,
                ApplicationType = type
            });

            return applications;
        }
    }
}
