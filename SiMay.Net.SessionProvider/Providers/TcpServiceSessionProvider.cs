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
    public class TcpServiceSessionProvider : SessionProvider
    {
        /// <summary>
        /// TcpSocket配置
        /// </summary>
        public TcpSocketSaeaServerConfiguration TcpSocketSaeaServerConfiguration { get; set; } = new TcpSocketSaeaServerConfiguration();

        TcpSocketSaeaServer _server;
        internal TcpServiceSessionProvider(SessionProviderOptions options)
        {
            ApplicationConfiguartion.SetOptions(options);

            TcpSocketSaeaServerConfiguration.AppKeepAlive = true;
            TcpSocketSaeaServerConfiguration.CompressTransferFromPacket = false;
            TcpSocketSaeaServerConfiguration.PendingConnectionBacklog = ApplicationConfiguartion.Options.PendingConnectionBacklog;

            _server = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, TcpSocketSaeaServerConfiguration, (notify, session) =>
             {
                 switch (notify)
                 {
                     case TcpSessionNotify.OnConnected:

                         SessionProviderContext sessionBased = new TcpServiceSessionContext(session);
                         this.Notification(sessionBased, TcpSessionNotify.OnConnected);
                         break;
                     case TcpSessionNotify.OnSend:
                         this.Notification(session.AppTokens.First().ConvertTo<SessionProviderContext>(), TcpSessionNotify.OnSend);
                         break;
                     case TcpSessionNotify.OnDataReceiveing:

                         var sessionProviderContext = session.AppTokens.First().ConvertTo<TcpServiceSessionContext>();
                         sessionProviderContext.OnProcess();

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
        public void StartSerivce() => _server.Listen(ApplicationConfiguartion.Options.ServiceIPEndPoint);

        public override void BroadcastAsync(byte[] data, int offset, int lenght) => _server.BroadcastAsync(data, offset, lenght);

        public override void CloseService() => _server.Dispose();

        public override void DisconnectAll() => _server.DisconnectAll(true);
    }
}
