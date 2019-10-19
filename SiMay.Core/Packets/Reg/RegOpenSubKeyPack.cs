using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegOpenSubKeyPack : BasePacket
    {
        public string Root { get; set; }
        public string NodePath { get; set; }
    }
}
