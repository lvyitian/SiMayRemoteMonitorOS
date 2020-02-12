using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class MainSessionProviderService
    {

        /// <summary>
        /// 日志输出
        /// </summary>
        public event Action<string> LogOutputEventHandler;

        private TcpSocketSaeaServer _tcpSaeaServer;
        private IList<Tuple<long, long>> _appServiceChannels = new List<Tuple<long, long>>();
        private IDictionary<long, TcpSessionChannelDispatcher> _dispatchers = new Dictionary<long, TcpSessionChannelDispatcher>();
        public bool StartService()
        {
            var serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.ReuseAddress = false;
            serverConfig.KeepAlive = true;
            serverConfig.KeepAliveInterval = 5000;
            serverConfig.KeepAliveSpanTime = 1000;
            serverConfig.PendingConnectionBacklog = 0;

            var ipe = new IPEndPoint(IPAddress.Parse(ApplicationConfiguartion.LocalAddress), ApplicationConfiguartion.ServicePort);

            _tcpSaeaServer = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Full, serverConfig, (notify, session) =>
            {
                switch (notify)
                {
                    case TcpSocketCompletionNotify.OnConnected:

                        ApportionDispatcher apportionDispatcher = new ApportionDispatcher();
                        apportionDispatcher.ApportionTypeHandlerEvent += ApportionTypeHandlerEvent;
                        apportionDispatcher.SetSession(session);

                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        this.OnDataReceiveingHandler(session);
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        this.OnClosedHandler(session);
                        break;
                    default:
                        break;
                }

            });
            return true;
        }

        private void OnDataReceiveingHandler(TcpSocketSaeaSession session)
        {
            byte[] data = new byte[session.ReceiveBytesTransferred];
            Array.Copy(session.CompletedBuffer, 0, data, 0, data.Length);

            var dispatcher = session.AppTokens[SysContanct.INDEX_WORKER].ConvertTo<DispatcherBase>();
            dispatcher.ListByteBuffer.AddRange(data);
            dispatcher.OnMessage();
        }

        private void OnClosedHandler(TcpSocketSaeaSession session)
        {
            var closedDispatcher = session.AppTokens[SysContanct.INDEX_WORKER].ConvertTo<DispatcherBase>();
            closedDispatcher.OnClosed();

            if (closedDispatcher.ConnectionWorkType == ConnectionWorkType.MainServiceConnection)
            {
                var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_SESSION_CLOSED,
                    new SessionClosedPacket()
                    {
                        Id = closedDispatcher.DispatcherId
                    });

                //通知所有主控端离线
                _dispatchers
                    .Select(c => c.Value)
                    .Where(c => c.ConnectionWorkType == ConnectionWorkType.MainApplicationConnection)
                    .ForEach(c => c.SendTo(data));
            }

            var serviceWorkerChannelItem = this._appServiceChannels.FirstOrDefault(c => c.Item2 == closedDispatcher.DispatcherId);
            if (!serviceWorkerChannelItem.IsNull())
                _appServiceChannels.Remove(serviceWorkerChannelItem);

            if (_dispatchers.ContainsKey(closedDispatcher.DispatcherId))
                _dispatchers.Remove(closedDispatcher.DispatcherId);
        }

        private void ApportionTypeHandlerEvent(ApportionDispatcher apportionDispatcher, ConnectionWorkType type)
        {
            switch (type)
            {
                case ConnectionWorkType.MainServiceConnection:
                    this.MainServiceConnect(apportionDispatcher);
                    break;
                case ConnectionWorkType.MainApplicationConnection:
                    this.MainApplicationConnect(apportionDispatcher);
                    break;
                case ConnectionWorkType.ApplicationServiceConnection:
                    this.ApplicationServiceConnect(apportionDispatcher);
                    break;
                case ConnectionWorkType.ApplicationConnection:
                    this.ApplicationConnect(apportionDispatcher);
                    break;
                case ConnectionWorkType.None:
                    break;
                default:
                    break;
            }
        }

        private void MainServiceConnect(ApportionDispatcher apportionDispatcher)
        {
            var mainServiceChannelDispatcher = apportionDispatcher.CreateMainServiceChannelDispatcher(_dispatchers);

            var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_SESSION,
                new SessionPacket()
                {
                    SessionItems = new SessionItemPacket[]
                    {
                        new SessionItemPacket(){
                            Id = mainServiceChannelDispatcher.DispatcherId,
                            ACKPacketData = mainServiceChannelDispatcher.ACKPacketData
                        }
                    }
                });
            //通知所有主控端上线
            _dispatchers
                .Select(c => c.Value)
                .Where(c => c.ConnectionWorkType == ConnectionWorkType.MainApplicationConnection)
                .ForEach(c => c.SendTo(data));

            _dispatchers.Add(mainServiceChannelDispatcher.DispatcherId, mainServiceChannelDispatcher);
        }

        private void MainApplicationConnect(ApportionDispatcher apportionDispatcher)
        {
            var accessId = apportionDispatcher.GetAccessId();
            var mainappChannelDispatcher = apportionDispatcher.CreateMainApplicationChannelDispatcher(_dispatchers);
            if (!_dispatchers.ContainsKey(accessId))//可能重连太快
            {
                //主控端使用自身AccessId作索引
                _dispatchers.Add(accessId, mainappChannelDispatcher);
            }
            else
            {
                var aboutOfCloseDispatcher = _dispatchers[accessId];

                var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_LOGOUT,
                    new LogOutPacket()
                    {
                        Message = "已有相同Id的主控端登陆，你已被登出!"
                    });
                aboutOfCloseDispatcher.SendTo(data);
                aboutOfCloseDispatcher.CloseSession();
            }
        }

        private void ApplicationServiceConnect(ApportionDispatcher apportionDispatcher)
        {
            //找到相应主控端连接
            TcpSessionChannelDispatcher dispatcher;
            if (_dispatchers.TryGetValue(apportionDispatcher.GetAccessId(), out dispatcher))
            {
                var appWorkerConnectionDispatcher = apportionDispatcher.CreateApplicationWorkerChannelDispatcher(_dispatchers, ConnectionWorkType.ApplicationServiceConnection);
                this._appServiceChannels.Add(new Tuple<long, long>(apportionDispatcher.GetAccessId(), appWorkerConnectionDispatcher.DispatcherId));

                var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_APPWORK);
                dispatcher.SendTo(data);
            }
        }

        private void ApplicationConnect(ApportionDispatcher apportionDispatcher)
        {
            TcpSessionChannelDispatcher dispatcher;
            var serviceWorkerChannelItem = this._appServiceChannels.FirstOrDefault(c => c.Item1 == apportionDispatcher.GetAccessId());
            if (!serviceWorkerChannelItem.IsNull() && _dispatchers.TryGetValue(serviceWorkerChannelItem.Item2, out dispatcher))
            {
                var serviceChannelDispatcher = dispatcher.ConvertTo<TcpSessionApplicationWorkerConnection>();
                var appChannelDispatcher = apportionDispatcher.CreateApplicationWorkerChannelDispatcher(_dispatchers, ConnectionWorkType.ApplicationConnection);
                if (!serviceChannelDispatcher.IsJoin)
                {
                    serviceChannelDispatcher.Join(appChannelDispatcher);
                    serviceChannelDispatcher.OnMessage();
                    appChannelDispatcher.OnMessage();
                }
                this._appServiceChannels.Remove(serviceWorkerChannelItem);

                _dispatchers.Add(appChannelDispatcher.DispatcherId, appChannelDispatcher);
            }
        }
    }
}
