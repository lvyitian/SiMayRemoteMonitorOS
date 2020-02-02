using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegDeleteValuePack : EntitySerializerBase
    {
        public string Root { get; set; }
        public string NodePath { get; set; }
        public string ValueName { get; set; }
    }
}
