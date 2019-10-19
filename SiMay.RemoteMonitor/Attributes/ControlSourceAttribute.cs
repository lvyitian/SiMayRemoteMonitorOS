using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteMonitor.Attributes
{
    public class ControlAppAttribute : Attribute
    {
        /// <summary>
        /// 应用特性
        /// </summary>
        /// <param name="rank">序号</param>
        /// <param name="name">组件名称</param>
        /// <param name="ctrlKey">组件ID</param>
        /// <param name="resourceName">图片资源名称</param>
        public ControlAppAttribute(int rank, string name, string ctrlKey, string resourceName = "", bool showOnTools = true)
        {
            Rank = rank;
            Name = name;
            ContorlKey = ctrlKey;
            ResourceName = resourceName;
            ShowOnTools = showOnTools;
        }
        public int Rank { get; set; }
        public string Name { get; set; }
        public string ContorlKey { get; set; }
        public string ResourceName { get; set; }
        public bool ShowOnTools { get; set; } = true;
    }
}
