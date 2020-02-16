using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.TcpConfiguration;

namespace SiMay.Net.SessionProvider.Providers
{
    public class TcpSocketSessionProvider : SessionProvider
    {
        TcpSocketSaeaServer _server;
        internal TcpSocketSessionProvider(SessionProviderOptions options)
        {
            ApplicationConfiguartion.SetOptions(options);
            ;
            var serverConfig = new TcpSocketSaeaServerConfiguration();

            serverConfig.AppKeepAlive = true;
            serverConfig.CompressTransferFromPacket = false;
            serverConfig.PendingConnectionBacklog = ApplicationConfiguartion.Options.PendingConnectionBacklog;

            _server = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, serverConfig, (notify, session) =>
             {
                 switch (notify)
                 {
                     case TcpSessionNotify.OnConnected:

                         SessionProviderContext sessionBased = new TcpSocketSessionContext(session);
                         session.AppTokens = new object[] {
                             sessionBased
                         };
                         this.SessionNotify(sessionBased, TcpSessionNotify.OnConnected);
                         break;
                     case TcpSessionNotify.OnSend:
                         this.SessionNotify(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnSend);
                         break;
                     case TcpSessionNotify.OnDataReceiveing:
                         this.SessionNotify(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnDataReceiveing);
                         break;
                     case TcpSessionNotify.OnDataReceived:
                         this.SessionNotify(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnDataReceived);
                         break;
                     case TcpSessionNotify.OnClosed:
                         this.SessionNotify(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnClosed);
                         break;
                     default:
                         break;
                 }

             });
        }
        public override void StartSerivce()
        {
            _server.Listen(ApplicationConfiguartion.Options.ServiceIPEndPoint);
        }

        public override void BroadcastAsync(byte[] data) => _server.BroadcastAsync(data);

        public override void BroadcastAsync(byte[] data, int offset, int lenght) => _server.BroadcastAsync(data, offset, lenght);

        public override void CloseService() => _server.Dispose();

        public override void DisconnectAll() => _server.DisconnectAll(true);
    }
}
