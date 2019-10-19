using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Screen
{
    public class ScreenSetClipoardPack : BasePacket
    {
        public string Text { get; set; }
    }
}
