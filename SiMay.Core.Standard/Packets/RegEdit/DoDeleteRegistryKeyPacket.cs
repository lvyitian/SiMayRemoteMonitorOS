using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class DoDeleteRegistryKeyPacket : EntitySerializerBase
    {
        public string ParentPath { get; set; }

        public string KeyName { get; set; }
    }
}
