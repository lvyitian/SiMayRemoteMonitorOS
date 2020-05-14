using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SuperWebSocket
{
    internal class Environment
    {
        internal const string NewLine = "\r\n";
    }

    internal class ReceivedDataBufferLength
    {
        internal const long Value = 1024 * 1024 * 10; //  1024 * 512
    }

    public class WebSocketConfigure
    {
        public IPAddress IPAddress;
        public short Port;
    }

    public class ServerConfigure : WebSocketConfigure
    {
        private int _listenCount = 100;
        public int ListenCount
        {
            get { return _listenCount; }
            set { _listenCount = value; }
        }

    }

    public class ClientConfigure : WebSocketConfigure
    {
        private int _timeout = 200;
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
    }


}
