using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public interface IWebSocketServer
    {
        string Id { get; }

        string Name { get; }

        WebSocketServerState State { get; }

        void Start();
        void Close();
        void SendData(string Id, byte[] Data);
    }
}
