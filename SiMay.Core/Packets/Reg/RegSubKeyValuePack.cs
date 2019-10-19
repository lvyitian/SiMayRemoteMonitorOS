using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegSubKeyValuePack : BasePacket
    {
        public string[] SubKeyNames { get; set; }
        public RegValueItem[] Values { get; set; }
    }
}
