using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public class MainApplicationAdapterHandler : MainApplicationBaseAdapterHandler
    {
        /// <summary>
        /// 当有数据上传时
        /// </summary>
        public event Action<SessionProviderContext> OnTransmitHandlerEvent;

        /// <summary>
        /// 当正在接收数据时
        /// </summary>

        public event Action<SessionProviderContext> OnReceiveHandlerEvent;

        /// <summary>
        /// 上线登陆处理事件
        /// </summary>
        public event Action<SessionSyncContext> OnLoginHandlerEvent;

        /// <summary>
        /// 上线信息更新事件
        /// </summary>
        public event Action<SessionSyncContext> OnLoginUpdateHandlerEvent;

        /// <summary>
        /// 离线处理事件
        /// </summary>
        public event Action<SessionSyncContext> OnLogOutHandlerEvent;

        /// <summary>
        /// 代理协议事件
        /// </summary>
        public event Action<ProxyProviderNotify, EventArgs> OnProxyNotifyHandlerEvent;

        /// <summary>
        /// 当应用被创建
        /// </summary>
        public event Action<IApplication> OnApplicationCreatedEventHandler;


        /// <summary>
        /// 监听日志事件
        /// </summary>
        public event Action<string, LogSeverityLevel> OnLogHandlerEvent;

        /// <summary>
        /// 主线程同步上下文
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// 会话提供对象
        /// </summary>
        public SessionProvider SessionProvider { get; set; }

        /// <summary>
        /// 主连接同步上下文
        /// </summary>
        public List<SessionSyncContext> SessionSyncContexts { get; set; } = new List<SessionSyncContext>();

        /// <summary>
        /// 中断任务上下文列表
        /// </summary>
        private List<SuspendTaskContext> _suspendTaskContexts = new List<SuspendTaskContext>();

        /// <summary>
        /// 是否已启动
        /// </summary>
        bool _launch;

        public MainApplicationAdapterHandler(AbstractAppConfigBase config)
        {
            if (_launch)
                return;

            _launch = true;

            AppConfiguration.SysConfig = config;

            ThreadHelper.CreateThread(ApplicationResetThread, true);
        }

        public void StartApp()
        {
            var providerType = int.Parse(AppConfiguration.SessionMode).ConvertTo<SessionProviderType>();

            string ip = providerType == SessionProviderType.TcpServiceSession
                ? AppConfiguration.IPAddress
                : AppConfiguration.ServiceIPAddress;

            int port = providerType == SessionProviderType.TcpServiceSession
                ? AppConfiguration.Port
                : AppConfiguration.ServicePort;

            AppConfiguration.UseAccessId = !AppConfiguration.EnabledAnonyMous ? AppConfiguration.AccessId : DateTime.Now.ToFileTimeUtc();

            int maxConnectCount = AppConfiguration.MaxConnectCount;

            var providerOptions = new SessionProviderOptions
            {
                ServiceIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port),
                PendingConnectionBacklog = maxConnectCount,
                AccessId = AppConfiguration.UseAccessId,//暂时使用UTC时间作为主控端标识
                MainAppAccessKey = AppConfiguration.MainAppAccessKey,
                MaxPacketSize = 1024 * 1024 * 2,
                AccessKey = long.Parse(AppConfiguration.AccessKey),
                SessionProviderType = providerType
            };

            if (providerType == SessionProviderType.TcpServiceSession)
            {
                if (StartServiceProvider(providerOptions))
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统端口 {port} 监听成功!", LogSeverityLevel.Information);
                else
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统端口 {port} 启动失败,请检查配置!", LogSeverityLevel.Warning);
            }
            else
            {
                if (StartProxySessionProvider(providerOptions))
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统初始化成功!", LogSeverityLevel.Information);
                else
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统初始化发生错误，请注意检查配置!", LogSeverityLevel.Warning);
            }

            bool StartServiceProvider(SessionProviderOptions options)
            {
                var tcpSessionProvider = SessionProviderFactory.CreateTcpServiceSessionProvider(options); ;
                tcpSessionProvider.NotificationEventHandler += OnNotifyProc;
                SessionProvider = tcpSessionProvider;
                try
                {
                    tcpSessionProvider.StartSerivce();
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return false;
                }
            }

            bool StartProxySessionProvider(SessionProviderOptions options)
            {
                var proxySessionProvider = SessionProviderFactory.CreateProxySessionProvider(options);
                proxySessionProvider.NotificationEventHandler += OnNotifyProc;
                proxySessionProvider.ProxyProviderNotify += OnProxyNotifyHandlerEvent;
                SessionProvider = proxySessionProvider;

                try
                {
                    proxySessionProvider.StartSerivce();
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 通信完成消息处理方法
        /// </summary>
        /// <param name="session"></param>
        /// <param name="notify"></param>
        private void OnNotifyProc(SessionProviderContext session, TcpSessionNotify notify)
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
                            //先分配好工作类型，等待工作指令分配新的工作类型
                            session.AppTokens = new object[SysConstants.INDEX_COUNT]
                            {
                                ConnectionWorkType.NONE,//未经验证的状态
                                null
                            };
                            break;
                        case TcpSessionNotify.OnSend:
                            //耗时操作会导致性能严重降低
                            this.OnTransmitHandlerEvent?.Invoke(session);
                            break;
                        case TcpSessionNotify.OnDataReceiveing:
                            //耗时操作会导致性能严重降低
                            this.OnReceiveHandlerEvent?.Invoke(session);
                            break;
                        case TcpSessionNotify.OnDataReceived:
                            this.OnReceiveComplete(session);
                            break;
                        case TcpSessionNotify.OnClosed:
                            this.OnClosed(session);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                }
            }
        }

        private void OnReceiveComplete(SessionProviderContext session)
        {
            // Tokens参数说明
            // [0]为该连接工作类型，MainWork为主连接，Work工作连接，NONE为未知连接
            // [1]如果连接为Work类型，则是消息处理器，否则是主连接上下文对象
            var appTokens = session.AppTokens;
            var sessionWorkType = appTokens[SysConstants.INDEX_WORKTYPE].ConvertTo<ConnectionWorkType>();

            if (sessionWorkType == ConnectionWorkType.WORKCON)
            {
                //消息传给消息处理器,由消息处理器所在App进行处理
                var app = appTokens[SysConstants.INDEX_WORKER].ConvertTo<ApplicationAdapterHandler>();
                if (app.WhetherClose)
                    return;
                app.HandlerBinder.InvokePacketHandler(session, session.GetMessageHead(), app);
            }
            else if (sessionWorkType == ConnectionWorkType.MAINCON)
                this.HandlerBinder.InvokePacketHandler(session, session.GetMessageHead(), this);
            else if (sessionWorkType == ConnectionWorkType.NONE) //未经过验证的连接的消息只能进入该方法块处理，连接密码验证正确才能正式处理消息
            {
                switch (session.GetMessageHead())
                {
                    case MessageHead.C_GLOBAL_CONNECT://连接确认包
                        this.ValiditySession(session);
                        break;
                    default://接收到其他数据包的处理
                        session.SessionClose();//伪造包,断开连接
                        break;
                }
            }
        }

        /// <summary>
        /// 确认连接包
        /// </summary>
        /// <param name="session"></param>
        private void ValiditySession(SessionProviderContext session)
        {
            var ack = session.GetMessageEntity<AckPack>();
            long accessKey = ack.AccessKey;
            if (accessKey != int.Parse(AppConfiguration.ConnectPassWord))
            {
                session.SessionClose();
                return;
            }
            else
            {
                //连接密码验证通过，设置成为主连接，正式接收处理数据包
                session.AppTokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.MAINCON;

                if (!ack.AssemblyLoad)
                    SendServicePlugins(session);

                //告诉服务端一切就绪
                session.SendTo(MessageHead.S_GLOBAL_OK);
            }
        }

        /// <summary>
        /// 中断任务重连线程
        /// </summary>
        private void ApplicationResetThread()
        {
            while (this._launch)
            {
                for (int i = 0; i < _suspendTaskContexts.Count; i++)
                {
                    SuspendTaskContext task = _suspendTaskContexts[i];

                    string id = task.AdapterHandler.IdentifyId.Split('|')[0];
                    var syncContext = SessionSyncContexts.FirstOrDefault(x => x.KeyDictions[SysConstants.IdentifyId].ConvertTo<string>() == id);

                    LogHelper.WriteErrorByCurrentMethod("beigin Reset--{0},{1},{2}".FormatTo(task.AdapterHandler.ApplicationKey, task.AdapterHandler.IdentifyId, id));

                    if (!syncContext.IsNull())
                    {
                        if (task.AdapterHandler.WhetherClose)
                        {
                            //窗口关闭将不再建立连接
                            _suspendTaskContexts.Remove(task); i--;
                            continue;
                        }
                        syncContext.Session.SendTo(MessageHead.S_MAIN_ACTIVATE_APPLICATIONSERVICE,
                            new ActivateServicePack()
                            {
                                ApplicationKey = task.AdapterHandler.ApplicationKey
                            });

                        LogHelper.WriteErrorByCurrentMethod("send reset command--{0},{1},{2}".FormatTo(task.AdapterHandler.ApplicationKey, task.AdapterHandler.IdentifyId, id));
                    }
                }
                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 加入重连线程
        /// </summary>
        /// <param name="context"></param>
        private void AddSuspendTaskContext(SuspendTaskContext context)
        {
            _suspendTaskContexts.Add(context);
            LogHelper.WriteErrorByCurrentMethod("Session Close--{0},{1}".FormatTo(context.AdapterHandler.ApplicationKey, context.AdapterHandler.IdentifyId));
        }

        /// <summary>
        /// 根据被控服务组合id寻找中断应用
        /// </summary>
        /// <param name="identifyId"></param>
        /// <returns></returns>
        private SuspendTaskContext FindOfSuspendTaskContext(string identifyId)
        {
            var task = _suspendTaskContexts
                .Where(x => x.AdapterHandler.IdentifyId.Split('|').FirstOrDefault() == identifyId)
                .FirstOrDefault();

            return task;
        }

        /// <summary>
        /// 移出重连线程
        /// </summary>
        /// <param name="identifyId"></param>
        /// <returns></returns>
        private bool RemoveSuspendTaskContext(string identifyId)
        {
            var task = _suspendTaskContexts.Where(x => x.AdapterHandler.IdentifyId.Split('|').FirstOrDefault().Equals(identifyId)).FirstOrDefault();
            if (task != null)
            {
                _suspendTaskContexts.Remove(task);
                LogHelper.WriteErrorByCurrentMethod("ResetTask Remove--{0},{1}".FormatTo(task.AdapterHandler.ApplicationKey, task.AdapterHandler.IdentifyId));
            }
            else
                return false;

            return true;
        }


        /// <summary>
        /// 启动App
        /// </summary>
        /// <param name="type"></param>
        /// <param name="session"></param>
        /// <param name="identifyId"></param>
        [PacketHandler(MessageHead.C_MAIN_ACTIVE_APP)]
        private void OnActivateStartApp(SessionProviderContext session)
        {
            var openControl = session.GetMessageEntity<ActivateApplicationPack>();
            string originName = openControl.OriginName;
            string appKey = openControl.ServiceKey;
            string identifyId = openControl.IdentifyId;
            //查找离线任务队列,如果有对应的任务则继续工作
            var task = FindOfSuspendTaskContext(identifyId);
            if (!task.IsNull())
            {
                //再发出重连命令后，如果使用者主动关闭消息处理器将不再建立连接
                if (task.AdapterHandler.WhetherClose)
                {
                    //通知远程释放资源
                    session.SendTo(MessageHead.S_GLOBAL_ONCLOSE);
                    return;
                }

                //将消息处理器与会话关联
                var tokens = session.AppTokens;
                tokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.WORKCON;
                tokens[SysConstants.INDEX_WORKER] = task.AdapterHandler;
                task.AdapterHandler.OriginName = originName;
                task.AdapterHandler.SetSession(session);
                task.AdapterHandler.ContinueTask(session);//继续任务

                RemoveSuspendTaskContext(identifyId);
            }
            else
            {
                var context = SysUtil.ApplicationTypes.FirstOrDefault(x => x.ApplicationKey.Equals(appKey));
                if (!context.IsNull())
                {

                    var appHandlerType = context.Type.GetAppAdapterHandlerType();
                    ApplicationAdapterHandler appHandlerBase = Activator.CreateInstance(appHandlerType).ConvertTo<ApplicationAdapterHandler>();
                    IApplication app = Activator.CreateInstance(context.Type).ConvertTo<IApplication>();

                    appHandlerBase.App = app;
                    appHandlerBase.IdentifyId = identifyId;
                    appHandlerBase.OriginName = originName;
                    appHandlerBase.ApplicationKey = context.Type.GetApplicationKey();
                    appHandlerBase.SetSession(session);

                    //每个应用至少标记一个应用处理器属性
                    var handlerFieder = context
                        .Type
                        .GetProperties()
                        .Single(c => !c.GetCustomAttribute<ApplicationAdapterHandlerAttribute>(true).IsNull());
                    handlerFieder.SetValue(app, appHandlerBase);

                    this.OnApplicationCreatedEventHandler?.Invoke(app);

                    //app.HandlerAdapter = handlerBase;
                    app.Start();

                    session.AppTokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.WORKCON;
                    session.AppTokens[SysConstants.INDEX_WORKER] = appHandlerBase;
                }
                else
                {
                    session.SessionClose();
                    LogHelper.WriteErrorByCurrentMethod("A working connection was closed because the control whose controlkey is :{0} could not be found!".FormatTo(appKey));
                    return;
                }
            }
        }


        ///// <summary>
        ///// 创建桌面记录任务
        ///// </summary>
        ///// <param name="session"></param>
        //[PacketHandler(MessageHead.C_MAIN_DESKTOPRECORD_OPEN)]
        //private void StartScreenRecordTaskHandler(SessionProviderContext session)
        //{
        //    string macName = session.GetMessage().ToUnicodeString();
        //    var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
        //    syncContext.KeyDictions[SysConstants.HasLaunchDesktopRecord] = true;//开启
        //    syncContext.KeyDictions[SysConstants.MachineName] = macName;//标识名(用计算机名作为唯一标识)

        //    session.SendTo(MessageHead.S_MAIN_DESKTOPRECORD_GETFRAME);
        //}

        ///// <summary>
        ///// 储存桌面记录信息
        ///// </summary>
        ///// <param name="session"></param>
        //[PacketHandler(MessageHead.C_MAIN_DESKTOPRECORD_FRAME)]
        //private void ScreenSaveHandler(SessionProviderContext session)
        //{
        //    var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
        //    var status = syncContext.KeyDictions[SysConstants.HasLaunchDesktopRecord].ConvertTo<bool>();
        //    var macName = syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>();

        //    if (!Directory.Exists(Path.Combine("DesktopRecord", macName)))
        //        Directory.CreateDirectory(Path.Combine("DesktopRecord", macName));

        //    using (var ms = new MemoryStream(session.GetMessage()))
        //    {
        //        string fileName = Path.Combine(Environment.CurrentDirectory, "DesktopRecord", macName, DateTime.Now.ToFileTime() + ".png");
        //        Image img = Image.FromStream(ms);
        //        img.Save(fileName);
        //        img.Dispose();
        //    }

        //    if (!status)
        //        return;

        //    session.SendTo(MessageHead.S_MAIN_DESKTOPRECORD_GETFRAME);
        //}

        ///// <summary>
        ///// 创建桌面视图
        ///// </summary>
        ///// <param name="session"></param>
        //[PacketHandler(MessageHead.C_MAIN_DESKTOPVIEW_CREATE)]
        //private void OnCreateDesktopView(SessionProviderContext session)
        //{
        //    var describePack = GetMessageEntity<DesktopViewDescribePack>(session);
        //    var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
        //    var view = this.OnCreateDesktopViewHandlerEvent?.Invoke(syncContext);
        //    if (view.IsNull())
        //        return;
        //    view.Caption = describePack.MachineName + "-(" + describePack.RemarkInformation + ")";
        //    syncContext.KeyDictions[SysConstants.DesktopView] = view;
        //}


        ///// <summary>
        ///// 显示桌面墙数据
        ///// </summary>
        ///// <param name="session"></param>
        //[PacketHandler(MessageHead.C_MAIN_DESKTOPVIEW_FRAME)]
        //private void PlayerDesktopImage(SessionProviderContext session)
        //{
        //    var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
        //    if (!syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView) ||
        //        syncContext.KeyDictions[SysConstants.DesktopView].IsNull())
        //        return;

        //    var frameData = session.GetMessageEntity<DesktopViewFramePack>();
        //    var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
        //    //if (frameData.InVisbleArea)
        //    //{
        //    using (var ms = new MemoryStream(frameData.ViewData))
        //        view.PlayerDekstopView(Image.FromStream(ms));
        //    //}

        //    GetDesktopViewFrame(syncContext);
        //}

        //public void GetDesktopViewFrame(SessionSyncContext syncContext)
        //{
        //    if (!syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView) ||
        //        syncContext.KeyDictions[SysConstants.DesktopView].IsNull())
        //        return;

        //    var view = syncContext.KeyDictions.GetValue(SysConstants.DesktopView).ConvertTo<IDesktopView>();
        //    view.SessionSyncContext.Session.SendTo(MessageHead.S_MAIN_DESKTOPVIEW_GETFRAME,
        //        new DesktopViewGetFramePack()
        //        {
        //            Height = view.Height,
        //            Width = view.Width
        //            //TimeSpan = this.ViewRefreshInterval
        //            //InVisbleArea = true
        //        });
        //}

        /// <summary>
        /// 登陆信息更改
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="login"></param>
        private void UpdateSyncContextHandler(SessionSyncContext syncContext, LoginPack login)
        {
            syncContext.KeyDictions[SysConstants.IPV4] = login.IPV4;
            syncContext.KeyDictions[SysConstants.MachineName] = login.MachineName;
            syncContext.KeyDictions[SysConstants.Remark] = login.Remark;
            syncContext.KeyDictions[SysConstants.ProcessorInfo] = login.ProcessorInfo;
            syncContext.KeyDictions[SysConstants.ProcessorCount] = login.ProcessorCount;
            syncContext.KeyDictions[SysConstants.MemorySize] = login.MemorySize;
            syncContext.KeyDictions[SysConstants.StartRunTime] = login.StartRunTime;
            syncContext.KeyDictions[SysConstants.ServiceVison] = login.ServiceVison;
            syncContext.KeyDictions[SysConstants.UserName] = login.UserName;
            syncContext.KeyDictions[SysConstants.OSVersion] = login.OSVersion;
            syncContext.KeyDictions[SysConstants.GroupName] = login.GroupName;
            syncContext.KeyDictions[SysConstants.ExistCameraDevice] = login.ExistCameraDevice;
            syncContext.KeyDictions[SysConstants.ExitsRecordDevice] = login.ExitsRecordDevice;
            syncContext.KeyDictions[SysConstants.ExitsPlayerDevice] = login.ExitsPlayerDevice;
            //syncContext.KeyDictions[SysConstants.OpenScreenRecord] = login.OpenScreenRecord;
            //syncContext.KeyDictions[SysConstants.OpenScreenWall] = login.OpenScreenWall;
            syncContext.KeyDictions[SysConstants.IdentifyId] = login.IdentifyId;
            //syncContext.KeyDictions[SysConstants.HasLaunchDesktopRecord] = false;//桌面记录状态
            //syncContext.KeyDictions[SysConstants.RecordHeight] = login.RecordHeight;//用于桌面记录的高
            //syncContext.KeyDictions[SysConstants.RecordWidth] = login.RecordWidth;//用于桌面记录宽
            //syncContext.KeyDictions[SysConstants.RecordSpanTime] = login.RecordSpanTime;
            //syncContext.KeyDictions[SysConstants.HasLoadServiceCOM] = login.InitialAssemblyLoad;

            this.OnLoginUpdateHandlerEvent?.Invoke(syncContext);
        }

        /// <summary>
        /// 添加用户信息到上线列表，并根据用户信息无人值守打开任务
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_LOGIN)]
        private void LoginHandler(SessionProviderContext session)
        {
            try
            {
                var login = session.GetMessageEntity<LoginPack>();
                if (!session.AppTokens[SysConstants.INDEX_WORKER].IsNull())//如果主连接同步对象存在，则对该对象更新
                {
                    this.UpdateSyncContextHandler(session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>(), login);
                    return;
                }

                var dictions = new Dictionary<string, object>()
                {
                    { SysConstants.IPV4, login.IPV4 },
                    { SysConstants.MachineName, login.MachineName },
                    { SysConstants.Remark, login.Remark },
                    { SysConstants.ProcessorInfo, login.ProcessorInfo },
                    { SysConstants.ProcessorCount, login.ProcessorCount },
                    { SysConstants.MemorySize, login.MemorySize },
                    { SysConstants.StartRunTime, login.StartRunTime },
                    { SysConstants.ServiceVison, login.ServiceVison },
                    { SysConstants.UserName, login.UserName },
                    { SysConstants.OSVersion, login.OSVersion },
                    { SysConstants.GroupName, login.GroupName },
                    { SysConstants.ExistCameraDevice, login.ExistCameraDevice },
                    { SysConstants.ExitsRecordDevice, login.ExitsRecordDevice },
                    { SysConstants.ExitsPlayerDevice, login.ExitsPlayerDevice },
                    //{ SysConstants.OpenScreenWall, login.OpenScreenWall },
                    { SysConstants.IdentifyId, login.IdentifyId }
                    //{ SysConstants.OpenScreenRecord, login.OpenScreenRecord },
                    //{ SysConstants.HasLaunchDesktopRecord, false },
                    //{ SysConstants.RecordHeight, login.RecordHeight },
                    //{ SysConstants.RecordWidth, login.RecordWidth },
                    //{ SysConstants.RecordSpanTime, login.RecordSpanTime },
                    //{ SysConstants.HasLoadServiceCOM, login.InitialAssemblyLoad }
                };
                var syncContext = new SessionSyncContext(session, dictions);
                SessionSyncContexts.Add(syncContext);
                session.AppTokens[SysConstants.INDEX_WORKER] = syncContext;

                ////是否开启桌面视图
                //if (syncContext.KeyDictions[SysConstants.OpenScreenWall].ConvertTo<bool>())
                //{
                //    SendTo(session, MessageHead.S_MAIN_CREATE_DESKTOPVIEW, new byte[] { 0 });//TODO : 强制创建视图,此处会触发载入插件
                //}

                this.OnLoginHandlerEvent?.Invoke(syncContext);

                this.OnLogHandlerEvent?.Invoke($"计算机:{syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>()}({syncContext.KeyDictions[SysConstants.Remark].ConvertTo<string>()}) -->已连接控制端!", LogSeverityLevel.Information);
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
                //可能是旧版本上线包
            }
        }

        /// <summary>
        /// 移除在线信息
        /// </summary>
        /// <param name="session"></param>
        private void OnClosed(SessionProviderContext session)
        {
            try
            {
                object[] arguments = session.AppTokens;
                var worktype = arguments[SysConstants.INDEX_WORKTYPE].ConvertTo<ConnectionWorkType>();
                if (worktype == ConnectionWorkType.WORKCON)
                {
                    var adapterHandler = arguments[SysConstants.INDEX_WORKER].ConvertTo<ApplicationAdapterHandler>();

                    if (adapterHandler.WhetherClose)//如果是手动结束任务
                        return;

                    adapterHandler.StateContext = "工作连接已断开,正在重新连接中....";
                    adapterHandler.SessionClosed(session);
                    //非手动结束任务，将该任务扔到重连线程中
                    AddSuspendTaskContext(new SuspendTaskContext()
                    {
                        DisconnectTime = DateTime.Now,
                        AdapterHandler = adapterHandler
                    });
                }
                else if (worktype == ConnectionWorkType.MAINCON)
                {
                    var syncContext = arguments[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
                    if (syncContext.IsNull())
                    {
                        LogHelper.WriteErrorByCurrentMethod("syncContext NULL");
                        return;
                    }
                    SessionSyncContexts.Remove(syncContext);

                    //if (syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView) && !syncContext.KeyDictions[SysConstants.DesktopView].IsNull())
                    //{
                    //    var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
                    //    view.CloseDesktopView();
                    //}

                    this.OnLogOutHandlerEvent?.Invoke(syncContext);

                    this.OnLogHandlerEvent?.Invoke($"计算机:{syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>()}({syncContext.KeyDictions[SysConstants.Remark].ConvertTo<string>()}) --已与控制端断开连接!", LogSeverityLevel.Warning);
                }
                else if (worktype == ConnectionWorkType.NONE)
                {
                    LogHelper.WriteErrorByCurrentMethod("NONE Session Close");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
        }
        /// <summary>
        /// 重新载入远程被控服务端
        /// </summary>
        /// <param name="syncContext"></param>
        public void RemoteServiceReload(SessionSyncContext syncContext)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_RELOADER);
        }

        public void RegetLoginInformation(SessionSyncContext syncContext)
        {
            syncContext.Session.SendTo(MessageHead.S_GLOBAL_OK);
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="syncContext"></param>
        public void InstallAutoStartService(SessionSyncContext syncContext)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_INSTANLL_SERVICE);
        }

        /// <summary>
        /// 卸载自动启动服务
        /// </summary>
        /// <param name="syncContext"></param>
        public void UnInStallAutoStartService(SessionSyncContext syncContext)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_UNINSTANLL_SERVICE);
        }
        /// <summary>
        /// 远程服务文件更新
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="updateType"></param>
        /// <param name="file"></param>
        /// <param name="url"></param>
        public void RemoteServiceUpdate(SessionSyncContext syncContext, RemoteUpdateType updateType, byte[] file, string url)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_UPDATE,
                new RemoteUpdatePack()
                {
                    UrlOrFileUpdate = updateType,
                    DownloadUrl = updateType == RemoteUpdateType.Url ? url : string.Empty,
                    FileDate = updateType == RemoteUpdateType.File ? file : new byte[0]
                });
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="groupName"></param>
        public void RemoteSetGroupName(SessionSyncContext syncContext, string groupName)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_GROUP, groupName);
        }

        ///// <summary>
        ///// 设置桌面视图
        ///// </summary>
        ///// <param name="syncContext"></param>
        //public bool SetSessionDesktopView(SessionSyncContext syncContext, IDesktopView desktopView)
        //{
        //    var machineName = syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>();
        //    var des = syncContext.KeyDictions[SysConstants.Remark].ConvertTo<string>();
        //    desktopView.Caption = machineName + $"-({des})";
        //    var result = syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView);
        //    if (result)
        //        return false;
        //    else
        //        syncContext.KeyDictions[SysConstants.DesktopView] = desktopView;

        //    return true;
        //    //SendTo(syncContext.Session, MessageHead.S_MAIN_CREATE_DESKTOPVIEW, new byte[] { (byte)(compel ? 0 : 1) });

        //}

        /////// <summary>
        /////// 关闭桌面视图
        /////// </summary>
        /////// <param name="syncContext"></param>
        //public void CloseDesktopView(SessionSyncContext syncContext)
        //{
        //    if (!syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView))
        //        return;
        //    var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
        //    view.CloseDesktopView();

        //    syncContext.KeyDictions.Remove(SysConstants.DesktopView);
        //}

        /// <summary>
        /// 远程打开Url
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="url"></param>
        public void RemoteOpenUrl(SessionSyncContext syncContext, string url)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_OPEN_WEBURL, url);
        }

        /// <summary>
        /// 设置系统会话状态
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="sessionType"></param>
        public void RemoteSetSessionState(SessionSyncContext syncContext, SystemSessionType sessionType)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_SESSION, new byte[] { (byte)sessionType });
        }

        /// <summary>
        /// 远程下载执行
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="url"></param>
        public void RemoteHttpDownloadExecute(SessionSyncContext syncContext, string url)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_HTTPDOWNLOAD, url);
        }

        /// <summary>
        /// 远程消息弹框
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        public void RemoteMessageBox(SessionSyncContext syncContext, string text, string title, MessageIcon icon)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_MESSAGEBOX,
                new MessagePack()
                {
                    MessageTitle = title,
                    MessageBody = text,
                    MessageIcon = (byte)icon
                });
        }

        /// <summary>
        /// 修改远程备注
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="remark"></param>
        public void RemoteSetRemarkInformation(SessionSyncContext syncContext, string remark)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_REMARK, remark);
        }

        /// <summary>
        /// 开打远程应用服务
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="appKey"></param>
        public void RemoteActiveService(SessionSyncContext syncContext, string appKey)
        {
            syncContext.Session.SendTo(MessageHead.S_MAIN_ACTIVATE_APPLICATIONSERVICE,
                new ActivateServicePack()
                {
                    ApplicationKey = appKey
                });
        }

        public override void Dispose()
        {
            this._launch = false;
            this.SessionProvider.CloseService();
            base.Dispose();
        }
    }
}
