using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider.Notify
{
    public enum SessionCompletedNotify
    {
        OnConnected,
        OnSend,
        OnRecv,
        OnReceived,
        OnClosed
    }
}
