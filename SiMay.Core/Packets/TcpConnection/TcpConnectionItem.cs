using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Core.Enums;

namespace SiMay.Core.Packets.TcpConnection
{
    public class TcpConnectionItem
    {
        public string ProcessName { get; set; }
        public string LocalAddress { get; set; }
        public string LocalPort { get; set; }
        public string RemoteAddress { get; set; }
        public string RemotePort { get; set; }
        public TcpConnectionState State { get; set; }
    }
}
