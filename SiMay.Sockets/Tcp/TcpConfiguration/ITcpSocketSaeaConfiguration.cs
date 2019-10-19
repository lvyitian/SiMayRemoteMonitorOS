using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp.TcpConfiguration
{
    public interface ITcpSocketSaeaConfiguration
    {
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        TimeSpan ReceiveTimeout { get; set; }
        TimeSpan SendTimeout { get; set; }
        bool NoDelay { get; set; }
        bool ReuseAddress { get; set; }
    }
}
