using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegDeleteSubKeyPack : BasePacket
    {
        public string Root { get; set; }
        public string NodePath { get; set; }
    }
}
