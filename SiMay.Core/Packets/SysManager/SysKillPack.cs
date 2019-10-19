using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.SysManager
{
    public class SysKillPack : BasePacket
    {
        public int[] ProcessIds { get; set; }
    }
}
