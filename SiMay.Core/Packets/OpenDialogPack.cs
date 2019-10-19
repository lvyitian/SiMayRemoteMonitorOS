using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ActiveAppPack : BasePacket
    {
        public string IdentifyId { get; set; }
        public string ServiceKey { get; set; }
        public string OriginName { get; set; }
    }
}
