using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ProcessItem : EntitySerializerBase
    {
        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public string WindowName { get; set; }

        public int WindowHandler { get; set; }

        public int ProcessMemorySize { get; set; }

        public int ProcessThreadCount { get; set; }
        public int SessionId { get; set; }
        public string User { get; set; }
        public string FilePath { get; set; }
    }
}
