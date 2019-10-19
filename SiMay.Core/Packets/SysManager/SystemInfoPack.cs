using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ProcessListPack : BasePacket
    {
        public ProcessItem[] ProcessList { get; set; }
    }
    public class SystemInfoPack : BasePacket
    {
        public SystemInfoItem[] SystemInfos { get; set; }
    }

    public class SystemOccupyPack : BasePacket
    {
        public string CpuUsage { get; set; }
        public string MemoryUsage { get; set; }
    }

    public class SystemInfoItem
    {
        public string ItemName { get; set; }
        public string Value { get; set; }
    }
}
