using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.Attributes
{
    /// <summary>
    /// 应用显示名称
    /// </summary>
    public class ApplicationNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ApplicationNameAttribute(string name)
            => Name = name;
    }
}
