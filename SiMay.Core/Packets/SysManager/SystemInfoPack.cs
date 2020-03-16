using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ProcessListPack : EntitySerializerBase
    {
        public ProcessItem[] ProcessList { get; set; }
    }
    public class SystemInfoPack : EntitySerializerBase
    {
        public SystemInfoItem[] SystemInfos { get; set; }
    }

    public class SystemOccupyPack : EntitySerializerBase
    {
        public string CpuUsage { get; set; }
        public string MemoryUsage { get; set; }
    }

    public class SystemInfoItem : EntitySerializerBase
    {
        public string ItemName { get; set; }
        public string Value { get; set; }
    }
    public class ServiceInfoPack : EntitySerializerBase
    {
        public ServiceItem[] ServiceList { get; set; }
    }
}
