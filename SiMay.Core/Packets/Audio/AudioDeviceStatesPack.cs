using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class AudioDeviceStatesPack : BasePacket
    {
        public bool PlayerEnable { get; set; }
        public bool RecordEnable { get; set; }
    }
}
