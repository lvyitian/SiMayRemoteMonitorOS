using System;
using System.Collections.Generic;
using SiMay.Sockets.Tcp.Client;
using System.Text;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System.Net;
using System.Linq;
using SiMay.Basic;

namespace SiMay.Net.SessionProvider.Providers
{
    public class TcpClientSessionProvider : SessionProvider
    {
        TcpSocketSaeaClientAgent _tcpSocketSaeaClientAgent;

        public TcpClientSessionProvider()
            : this(new TcpSocketSaeaClientConfiguration())
        {
        }

        public TcpClientSessionProvider(TcpSocketSaeaClientConfiguration clientConfiguration)
        {
            ApplicationConfiguartion.SetOptions(new SessionProviderOptions());
            _tcpSocketSaeaClientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfiguration,
                (notify, session) =>
            {
                switch (notify)
                {
                    case TcpSessionNotify.OnConnected:

                        TcpClientSessionContext sessionBased = new TcpClientSessionContext(session);
                        this.Notification(sessionBased, TcpSessionNotify.OnConnected);
                        break;
                    case TcpSessionNotify.OnSend:
                        this.Notification(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnSend);
                        break;
                    case TcpSessionNotify.OnDataReceiveing:

                        var sessionProviderContext = session.AppTokens.First().ConvertTo<TcpClientSessionContext>();
                        sessionProviderContext.OnMessage();

                        this.Notification(sessionProviderContext, TcpSessionNotify.OnDataReceiveing);
                        break;
                    case TcpSessionNotify.OnDataReceived:
                        this.Notification(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnDataReceived);
                        break;
                    case TcpSessionNotify.OnClosed:
                        this.Notification(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnClosed);
                        break;
                    default:
                        break;
                }
            });
        }

        public void ConnectAsync(IPEndPoint endPoint) => _tcpSocketSaeaClientAgent.ConnectToServer(endPoint);

        public override void BroadcastAsync(byte[] data, int offset, int lenght)
            => _tcpSocketSaeaClientAgent.BroadcastAsync(data, offset, lenght);

        public override void CloseService()
            => _tcpSocketSaeaClientAgent.Dispose();

        public override void DisconnectAll()
            => _tcpSocketSaeaClientAgent.DisconnectAll(true);
    }
}
