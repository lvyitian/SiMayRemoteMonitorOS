using SiMay.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class MessagePack : BasePacket
    {
        public byte MessageIcon { get; set; }

        public string MessageTitle { get; set; }

        public string MessageBody { get; set; }
    }
}
