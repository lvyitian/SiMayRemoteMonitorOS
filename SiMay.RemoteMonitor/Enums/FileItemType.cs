using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SiMay.RemoteMonitor.Enums
{
    public enum FileItemType
    {
        [Description("文件")]
        File,
        [Description("文件夹")]
        Directory,
        [Description("磁盘")]
        Disk
    }
}
