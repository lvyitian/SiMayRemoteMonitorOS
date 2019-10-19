using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegOperFinshPack : BasePacket
    {
        public bool Result { get; set; }
        public string Value { get; set; }
    }
}
