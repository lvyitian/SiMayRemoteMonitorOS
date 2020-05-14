using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public interface IWebSocketClient
    {
        string Id { get; }

        string Name { get; }

        bool IsDataMasked { get; }

        WebSocketClientState State { get; }

        void Close();

        void Connect(string ip, string port, string user, string password, string func, string data);
    }
}
