using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class DoCreateRegistryKeyPack : EntitySerializerBase
    {
        public string ParentPath { get; set; }
    }
}
