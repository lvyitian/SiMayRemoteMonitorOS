using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ServicePluginPack : EntitySerializerBase
    {
        public PluginItem[] Files { get; set; }
    }

    public class PluginItem : EntitySerializerBase
    {
        public string FileName { get; set; }
        public byte[] PayLoad { get; set; }
    }
}
