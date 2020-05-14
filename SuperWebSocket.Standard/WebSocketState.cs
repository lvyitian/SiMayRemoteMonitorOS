using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public enum WebSocketServerState
    {
        None,
        Started,
        Stoped,
        Disposed
    }

    public enum WebSocketClientState
    {
        None,        
        Connected,
        Disconnected
    }
    
}
