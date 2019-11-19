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

        public int PrimaryScreenIndex { get; set; }
        public MonitorItem[] Monitors { get; set; }
    }

    public class MonitorItem
    {
        public string DeviceName { get; set; }
        public bool Primary { get; set; }
    }
}
