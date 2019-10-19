using SiMay.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class AckPack : BasePacket
    {
        public long AccessKey { get; set; }
        public ConnectionWorkType Type { get; set; }
    }
}
