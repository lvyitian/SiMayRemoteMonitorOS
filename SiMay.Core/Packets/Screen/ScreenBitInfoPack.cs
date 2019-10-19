using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ScreenInitBitPack : BasePacket
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
