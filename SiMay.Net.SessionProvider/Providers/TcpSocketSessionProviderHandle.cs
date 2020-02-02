using SiMay.Net.SessionProvider.Delegate;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider.Providers
{
    public class TcpSocketSessionProviderHandle : SessionProvider
    {
        TcpSocketSaeaServer _server;
        SessionProviderOptions _options;
        internal TcpSocketSessionProviderHandle(
            SessionProviderOptions options,
            OnSessionNotify<SessionCompletedNotify, SessionProviderContext> onSessionNotifyProc)
            : base(onSessionNotifyProc)
        {

            _options = options;

            var serverConfig = new TcpSocketSaeaServerConfiguration();

            serverConfig.AppKeepAlive = true;
            serverConfig.CompressTransferFromPacket = false;
            serverConfig.PendingConnectionBacklog = options.PendingConnectionBacklog;

            _server = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, serverConfig, (notify, session) =>
             {
                 switch (notify)
                 {
                     case TcpSocketCompletionNotify.OnConnected:

                         var sessionBased = new TcpSocketSessionBased(session);

                         session.AppTokens = new object[]
                         {
                             sessionBased
                         };

                         _onSessionNotifyProc(SessionCompletedNotify.OnConnected, sessionBased);

                         break;
                     case TcpSocketCompletionNotify.OnSend:
                         _onSessionNotifyProc(SessionCompletedNotify.OnSend, session.AppTokens[0] as SessionProviderContext);
                         break;
                     case TcpSocketCompletionNotify.OnDataReceiveing:
                         _onSessionNotifyProc(SessionCompletedNotify.OnRecv, session.AppTokens[0] as SessionProviderContext);
                         break;
                     case TcpSocketCompletionNotify.OnDataReceived:
                         _onSessionNotifyProc(SessionCompletedNotify.OnReceived, session.AppTokens[0] as SessionProviderContext);
                         break;
                     case TcpSocketCompletionNotify.OnClosed:
                         _onSessionNotifyProc(SessionCompletedNotify.OnClosed, session.AppTokens[0] as SessionProviderContext);
                         break;
                     default:
                         break;
                 }

             });
        }
        public override void StartSerivce() =>
            _server.Listen(_options.ServiceIPEndPoint);
        public override void BroadcastAsync(byte[] data) =>
            _server.BroadcastAsync(data);

        public override void BroadcastAsync(byte[] data, int offset, int lenght) =>
            _server.BroadcastAsync(data, offset, lenght);

        public override void CloseService() =>
            _server.Dispose();

        public override void DisconnectAll() =>
            _server.DisconnectAll(true);
    }
}
