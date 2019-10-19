using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class DesktopViewDescribePack : BasePacket
    {
        public string MachineName { get; set; }
        public string RemarkInformation { get; set; }
    }
}
