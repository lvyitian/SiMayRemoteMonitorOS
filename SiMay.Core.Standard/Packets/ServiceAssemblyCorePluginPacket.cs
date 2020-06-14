using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class ServiceAssemblyCorePluginPacket : EntitySerializerBase
    {
        public AssemblyFileItem[] Files { get; set; }
    }

    public class AssemblyFileItem : EntitySerializerBase
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
    }
}
