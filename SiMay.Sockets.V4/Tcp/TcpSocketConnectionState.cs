using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp
{
    public enum TcpSocketConnectionState
    {
        None = 0,
        Connecting = 1,
        Connected = 2,
        Closed = 3,
    }
}
