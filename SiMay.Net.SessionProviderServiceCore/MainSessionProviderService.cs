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
using System.Threading;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class MainSessionProviderService
    {
        /// <summary>
        /// 日志输出
        /// </summary>
        public event Action<LogOutLevelType, string> LogOutputEventHandler;

        /// <summary>
        /// 连接事件
        /// </summary>
        public event Action<TcpSessionChannelDispatcher> OnConnectedEventHandler;

        /// <summary>
        /// 离线事件
        /// </summary>
        public event Action<TcpSessionChannelDispatcher> OnClosedEventHandler;

        /// <summary>
        /// 主线程同步上下文
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; set; }

        private TcpSocketSaeaServer _tcpSaeaServer;
        private IList<Tuple<long, long>> _appServiceChannels = new List<Tuple<long, long>>();
        private IDictionary<long, TcpSessionChannelDispatcher> _dispatchers = new Dictionary<long, TcpSessionChannelDispatcher>();
        public bool StartService(StartServiceOptions options)
        {
            ApplicationConfiguartion.SetOptions(options);

            var serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.ReuseAddress = false;
            serverConfig.KeepAlive = true;
            serverConfig.KeepAliveInterval = 5000;
            serverConfig.KeepAliveSpanTime = 1000;
            serverConfig.PendingConnectionBacklog = 0;

            var ipe = new IPEndPoint(IPAddress.Parse(ApplicationConfiguartion.Options.LocalAddress), ApplicationConfiguartion.Options.ServicePort);

            _tcpSaeaServer = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Full, serverConfig, (notify, session) =>
            {
                if (SynchronizationContext.IsNull())
                    NotifyProc(null);
                else
                    SynchronizationContext.Send(NotifyProc, null);

                void NotifyProc(object @object)
                {
                    try
                    {
                        switch (notify)
                        {
                            case TcpSessionNotify.OnConnected:

                                ApportionDispatcher apportionDispatcher = new ApportionDispatcher();
                                apportionDispatcher.ApportionTypeHandlerEvent += ApportionTypeHandlerEvent;
                                apportionDispatcher.LogOutputEventHandler += ApportionDispatcher_LogOutputEventHandler;
                                apportionDispatcher.SetSession(session);

                                break;
                            case TcpSessionNotify.OnSend:
                                break;
                            case TcpSessionNotify.OnDataReceiveing:
                                this.OnDataReceiveingHandler(session);
                                break;
                            case TcpSessionNotify.OnClosed:
                                this.OnClosedHandler(session);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteErrorByCurrentMethod(ex);
                    }
                }
            });

            try
            {
                _tcpSaeaServer.Listen(ipe);
                LogOutputEventHandler?.Invoke(LogOutLevelType.Debug, $"SiMay中间会话服务器监听 {ipe.Port} 端口启动成功!");
                return true;
            }
            catch (Exception ex)
            {
                LogOutputEventHandler?.Invoke(LogOutLevelType.Error, $"SiMay中间会话服务器启动发生错误,端口:{ipe.Port} 错误信息:{ex.Message}!");
                return false;
            }
        }

        private void ApportionDispatcher_LogOutputEventHandler(DispatcherBase dispatcher, LogOutLevelType level, string log)
        {
            LogOutputEventHandler?.Invoke(level, log);
        }

        private void OnDataReceiveingHandler(TcpSocketSaeaSession session)
        {
            byte[] data = session.CompletedBuffer.Copy(0, session.ReceiveBytesTransferred);
            var dispatcher = session.AppTokens[SysContanct.INDEX_WORKER].ConvertTo<DispatcherBase>();
            dispatcher.OnMessageBefore(data);
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

            if (closedDispatcher is ApportionDispatcher ofclosedApportionDispatcher)
            {
                ofclosedApportionDispatcher.ApportionTypeHandlerEvent -= ApportionTypeHandlerEvent;
                ofclosedApportionDispatcher.LogOutputEventHandler -= ApportionDispatcher_LogOutputEventHandler;
                ofclosedApportionDispatcher.Dispose();
            }

            if (closedDispatcher is TcpSessionChannelDispatcher tcpSessionChannelDispatcher)
                this.OnClosedEventHandler?.Invoke(tcpSessionChannelDispatcher);

            this.LogOutputEventHandler?.Invoke(LogOutLevelType.Warning, $"ID:{closedDispatcher.DispatcherId} 的连接已与服务器断开连接!");
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
            this.LogOutputEventHandler?.Invoke(LogOutLevelType.Debug, $"工作类型:{type.ToString()} 的连接已分配!");
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

            this.OnConnectedEventHandler?.Invoke(mainServiceChannelDispatcher);
            _dispatchers.Add(mainServiceChannelDispatcher.DispatcherId, mainServiceChannelDispatcher);
        }

        private void MainApplicationConnect(ApportionDispatcher apportionDispatcher)
        {
            var accessId = apportionDispatcher.GetAccessId();
            var mainappChannelDispatcher = apportionDispatcher.CreateMainApplicationChannelDispatcher(_dispatchers);
            if (_dispatchers.ContainsKey(accessId))//可能重连太快
            {
                var aboutOfCloseDispatcher = _dispatchers[accessId];

                var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_LOGOUT,
                    new LogOutPacket()
                    {
                        Message = "已有相同Id的主控端登陆，你已被登出"
                    });
                aboutOfCloseDispatcher.SendTo(data);
                aboutOfCloseDispatcher.CloseSession();//调用后底层会同步触发Closed事件释放连接的占用资源

                this.LogOutputEventHandler?.Invoke(LogOutLevelType.Debug, $"有相同Id:{accessId}的主控端登陆!");
            }
            _dispatchers.Add(accessId, mainappChannelDispatcher);

            this.OnConnectedEventHandler?.Invoke(mainappChannelDispatcher);
        }

        private void ApplicationServiceConnect(ApportionDispatcher apportionDispatcher)
        {
            var appWorkerConnectionDispatcher = apportionDispatcher.CreateApplicationWorkerChannelDispatcher(_dispatchers, ConnectionWorkType.ApplicationServiceConnection);

            appWorkerConnectionDispatcher.ListByteBuffer.AddRange(apportionDispatcher.GetACKPacketData().BuilderHeadPacket());

            if (apportionDispatcher.ListByteBuffer.Count > 0)
            {
                var bufferData = apportionDispatcher.ListByteBuffer.ToArray();
                appWorkerConnectionDispatcher.ListByteBuffer.AddRange(bufferData);
            }

            this._dispatchers.Add(appWorkerConnectionDispatcher.DispatcherId, appWorkerConnectionDispatcher);
            this._appServiceChannels.Add(new Tuple<long, long>(apportionDispatcher.GetAccessId(), appWorkerConnectionDispatcher.DispatcherId));
            this.OnConnectedEventHandler?.Invoke(appWorkerConnectionDispatcher);
            //找到相应主控端连接
            TcpSessionChannelDispatcher dispatcher;
            if (_dispatchers.TryGetValue(apportionDispatcher.GetAccessId(), out dispatcher))
            {
                var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_APPWORK);
                dispatcher.SendTo(data);
            }
            else
            {
                appWorkerConnectionDispatcher.CloseSession();
                this.LogOutputEventHandler?.Invoke(LogOutLevelType.Debug, $"应用服务工作连接未找到相应的主控端!");
            }
        }

        private void ApplicationConnect(ApportionDispatcher apportionDispatcher)
        {
            TcpSessionChannelDispatcher dispatcher = null;
            var serviceWorkerChannelItem = this._appServiceChannels.FirstOrDefault(c => c.Item1 == apportionDispatcher.GetAccessId());
            if (!serviceWorkerChannelItem.IsNull() && _dispatchers.TryGetValue(serviceWorkerChannelItem.Item2, out dispatcher))
            {
                this._appServiceChannels.Remove(serviceWorkerChannelItem);
                var serviceChannelDispatcher = dispatcher.ConvertTo<TcpSessionApplicationWorkerConnection>();
                var appChannelDispatcher = apportionDispatcher.CreateApplicationWorkerChannelDispatcher(_dispatchers, ConnectionWorkType.ApplicationConnection);

                if (apportionDispatcher.ListByteBuffer.Count > 0)
                {
                    var bufferData = apportionDispatcher.ListByteBuffer.ToArray();
                    appChannelDispatcher.ListByteBuffer.AddRange(bufferData);
                }

                this._dispatchers.Add(appChannelDispatcher.DispatcherId, appChannelDispatcher);
                this.OnConnectedEventHandler?.Invoke(appChannelDispatcher);

                if (!serviceChannelDispatcher.IsJoin)
                {
                    serviceChannelDispatcher.Join(appChannelDispatcher);
                    serviceChannelDispatcher.OnMessage();
                    appChannelDispatcher.OnMessage();
                }
                else appChannelDispatcher.CloseSession();
            }
            else
            {
                apportionDispatcher.CloseSession();
                this.LogOutputEventHandler?.Invoke(LogOutLevelType.Debug, $"主控端应用工作连接未找到可匹配的应用服务工作连接!");
            }
        }
    }
}
