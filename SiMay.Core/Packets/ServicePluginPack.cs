using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ServicePluginPack
    {
        public PluginItem[] Files { get; set; }
    }

    public class PluginItem
    {
        public string FileName { get; set; }
        public byte[] PayLoad { get; set; }
    }
}
