using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class DoDeleteRegistryValuePacket : EntitySerializerBase
    {
        public string KeyPath { get; set; }

        public string ValueName { get; set; }
    }
}
