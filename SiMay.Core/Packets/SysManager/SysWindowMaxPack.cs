using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.SysManager
{
    public class SysWindowMaxPack : BasePacket
    {
        public int State { get; set; }
        public int[] Handlers { get; set; }
    }
}
