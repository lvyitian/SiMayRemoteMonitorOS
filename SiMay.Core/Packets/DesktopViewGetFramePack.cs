using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class DesktopViewGetFramePack : BasePacket
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int TimeSpan { get; set; }
        public bool InVisbleArea { get; set; }
    }

    public class DesktopViewFramePack : BasePacket
    {
        public bool InVisbleArea { get; set; }
        public byte[] ViewData { get; set; }
    }
}
