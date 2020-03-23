using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    /// <summary>
    /// 应用图片资源名称
    /// </summary>
    public class AppResourceNameAttribute : Attribute
    {
        public string Name { get; set; }
        public AppResourceNameAttribute(string name)
            => Name = name;
    }
}
