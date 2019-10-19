using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;

namespace SiMay.Sockets.Tcp
{
    public class TcpSocketsFactory
    {
        public static TcpSocketSaeaClientAgent CreateClientAgent(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaClientConfiguration configuration,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> completetionNotify)
        {
            configuration._SessionIsService = false;
            return new TcpSocketSaeaClientAgent(saeaSessionType, configuration, completetionNotify);
        }

        public static TcpSocketSaeaServer CreateServerAgent(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaServerConfiguration configuration,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> completetionNotify)
        {
            configuration._SessionIsService = true;
            return new TcpSocketSaeaServer(saeaSessionType, configuration, completetionNotify);
        }
    }
}
