using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.TcpConnection
{
    public class TcpConnectionPack : EntitySerializerBase
    {
        public TcpConnectionItem[] TcpConnections { get; set; }
    }

    public class KillTcpConnectionPack : EntitySerializerBase
    {
        public KillTcpConnectionItem[] Kills { get; set; }
    }
    public class KillTcpConnectionItem : EntitySerializerBase
    {
        public string LocalAddress { get; set; }

        public string LocalPort { get; set; }

        public string RemoteAddress { get; set; }

        public string RemotePort { get; set; }
    }
}
