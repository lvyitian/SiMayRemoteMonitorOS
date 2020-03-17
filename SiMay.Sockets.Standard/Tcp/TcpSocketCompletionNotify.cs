using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp
{
    public enum TcpSessionNotify
    {
        OnConnected,
        OnSend,
        OnDataReceiveing,
        OnDataReceived,
        OnClosed,
    }
}
