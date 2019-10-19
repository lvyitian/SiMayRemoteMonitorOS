using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp.TcpConfiguration
{
    public class TcpSocketSaeaServerConfiguration : TcpSocketConfigurationBase
    {
        public TcpSocketSaeaServerConfiguration()
        {
            PendingConnectionBacklog = 200;//服务器连接挂起队列数量
        }
        public int PendingConnectionBacklog { get; set; }
    }
}
