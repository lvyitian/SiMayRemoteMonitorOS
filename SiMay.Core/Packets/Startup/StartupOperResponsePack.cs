using SiMay.Core.Packets.Startup.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Startup
{
    public class StartupOperResponsePack : BasePacket
    {
        public OperFlag OperFlag { get; set; }
        public string Msg { get; set; }
        public bool Successed { get; set; }
    }
}
