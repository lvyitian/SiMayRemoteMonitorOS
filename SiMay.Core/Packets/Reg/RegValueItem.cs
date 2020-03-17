using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegValueItem : EntitySerializerBase
    {
        public string ValueName { get; set; }
        public string Value { get; set; }
    }
}
