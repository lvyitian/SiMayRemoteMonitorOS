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
        public event Func<IApplication, bool> OnApplicationCreatedEventHandler;


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
        public SessionProvider SessionProvider { get; private set; }

        /// <summary>
        /// 主连接同步上下文
        /// </summary>
        public List<SessionSyncContext> SessionSyncContexts { get; } = new List<SessionSyncContext>();

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
            TaskScheduleTrigger.StarSchedule(10);
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
                //消息传给消息适配器,由消息适配器进行处理，通过事件反馈数据到展示层
                var adapter = appTokens[SysConstants.INDEX_WORKER].ConvertTo<ApplicationAdapterHandler>();
                if (adapter.IsManualClose())
                    return;
                adapter.HandlerBinder.InvokePacketHandler(session, session.GetMessageHead(), adapter);
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
            var ack = session.GetMessageEntity<AcknowledPacket>();
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
        /// 启动App
        /// </summary>
        /// <param name="type"></param>
        /// <param name="session"></param>
        /// <param name="identifyId"></param>
        [PacketHandler(MessageHead.C_MAIN_ACTIVE_APP)]
        private void OnActivateStartApp(SessionProviderContext session)
        {
            lock (this)
            {
                var activateResponse = session.GetMessageEntity<ActivateApplicationPack>();
                string originName = activateResponse.OriginName;
                string appKey = activateResponse.ApplicationKey;
                string identifyId = activateResponse.IdentifyId;

                var applicationName = activateResponse.ActivatedCommandText.Split('.')[0];

                //离线适配器TaskName格式:identifyId,APP.AdapterKey，其中 identifyId 表示被控端唯一标识，用于区分重连任务中不同被控端,APP为应用名称，AdapterKey为适配器Id
                //等待应用TaskName格式:identifyId,APP
                //优先级说明:等待应用优先匹配，应用创建时如有多个适配器，第一个适配器完成初始化后会被创建为等待应用加入任务调度队列，直至所有适配器连接完成，否则超时应用会被判定创建失败。

                //查找任务调度队列,如果有对应的任务则继续工作
                if (TaskScheduleTrigger.FindScheduleTask(c => c.TaskName.Contains(identifyId) && (c.TaskName.Split(',')[1].Equals(applicationName, StringComparison.OrdinalIgnoreCase) || c.TaskName.Split(',')[1].Equals(activateResponse.ActivatedCommandText, StringComparison.OrdinalIgnoreCase)), out var taskSchedule))
                {
                    //如果是匹配到了离线适配器
                    if (taskSchedule.TaskName.Equals($"{identifyId},{activateResponse.ActivatedCommandText}") && taskSchedule is ICustomEvent task)
                        task.Invoke(this, new SuspendTaskResumEventArgs()
                        {
                            Session = session
                        });
                    else if (taskSchedule.TaskName.Equals($"{identifyId},{applicationName}") && taskSchedule is ApplicationCreatingTimeOutSuspendTaskContext creatingTimeOutContext)
                    {
                        var application = creatingTimeOutContext.Application;
                        var property = application.GetApplicationAdapterPropertyByKey(appKey);
                        if (property.IsNull())
                            throw new ArgumentNullException("adapter not found!");

                        var adapter = Activator.CreateInstance(property.PropertyType).ConvertTo<ApplicationAdapterHandler>();
                        adapter.App = application;
                        adapter.IdentifyId = identifyId;
                        adapter.OriginName = originName;
                        adapter.SetSession(session);
                        //property.SetValue(application, adapter);

                        if (ApplicationReadyExamine(adapter, application))
                            TaskScheduleTrigger.RemoveScheduleTask(taskSchedule);
                    }
                    else
                        throw new ApplicationException();
                }
                else
                {
                    //查找应用
                    var context = SysUtil.ApplicationTypes.FirstOrDefault(c => c.Type.Name.Equals(applicationName, StringComparison.OrdinalIgnoreCase));
                    if (!context.IsNull())
                    {
                        //根据appKey查找该应用适配器
                        var appAdapterProperty = context.Type.GetApplicationAdapterPropertyByKey(appKey);

                        if (appAdapterProperty.IsNull())
                            throw new ApplicationException("adapter not declaration!");

                        ApplicationAdapterHandler appHandlerBase = Activator.CreateInstance(appAdapterProperty.PropertyType).ConvertTo<ApplicationAdapterHandler>();
                        IApplication app = Activator.CreateInstance(context.Type).ConvertTo<IApplication>();

                        appHandlerBase.App = app;
                        appHandlerBase.IdentifyId = identifyId;
                        appHandlerBase.OriginName = originName;
                        //appHandlerBase.ApplicationKey = context.Type.GetApplicationKey();
                        appHandlerBase.SetSession(session);

                        ApplicationReadyExamine(appHandlerBase, app);
                    }
                    else
                    {
                        session.SessionClose();
                        LogHelper.WriteErrorByCurrentMethod("a working connection was closed because the control whose appkey is :{0} could not be found!".FormatTo(appKey));
                        return;
                    }


                }

                //应用资源情况检查
                bool ApplicationReadyExamine(ApplicationAdapterHandler adapter, IApplication app)
                {
                    var handlerFieders = app
                        .GetApplicationAdapterProperty()
                        .ToDictionary(key => key.PropertyType.GetApplicationKey(), val => val);

                    if (handlerFieders.ContainsKey(appKey) && handlerFieders.TryGetValue(appKey, out var property))
                        property.SetValue(app, adapter);
                    else
                        throw new ApplicationException();

                    //检查所有适配器属性
                    var prepareCompleted = handlerFieders.Where(c => !c.Value.GetValue(app).IsNull()/* && c.Value.GetValue(app).ConvertTo<ApplicationAdapterHandler>().AttachedConnection*/);
                    if (prepareCompleted.Count() != handlerFieders.Count)
                    {
                        //创建超时任务
                        TaskScheduleTrigger.AddScheduleTask(new ApplicationCreatingTimeOutSuspendTaskContext()
                        {
                            Application = app,
                            TaskName = $"{identifyId},{app.GetType().Name}"
                        });
                        return false;
                    }

                    var successed = this.OnApplicationCreatedEventHandler.Invoke(app);
                    if (successed)
                    {
                        //app.HandlerAdapter = handlerBase;
                        app.Start();

                        session.AppTokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.WORKCON;
                        session.AppTokens[SysConstants.INDEX_WORKER] = adapter;
                    }
                    else
                        session.SessionClose();

                    return successed;
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
        private void UpdateSyncContextHandler(SessionSyncContext syncContext, LoginPacket login)
        {
            syncContext[SysConstants.IPV4] = login.IPV4;
            syncContext[SysConstants.MachineName] = login.MachineName;
            syncContext[SysConstants.Remark] = login.Remark;
            syncContext[SysConstants.ProcessorInfo] = login.ProcessorInfo;
            syncContext[SysConstants.ProcessorCount] = login.ProcessorCount;
            syncContext[SysConstants.MemorySize] = login.MemorySize;
            syncContext[SysConstants.StartRunTime] = login.StartRunTime;
            syncContext[SysConstants.ServiceVison] = login.ServiceVison;
            syncContext[SysConstants.UserName] = login.UserName;
            syncContext[SysConstants.OSVersion] = login.OSVersion;
            syncContext[SysConstants.GroupName] = login.GroupName;
            syncContext[SysConstants.ExistCameraDevice] = login.ExistCameraDevice;
            syncContext[SysConstants.ExitsRecordDevice] = login.ExitsRecordDevice;
            syncContext[SysConstants.ExitsPlayerDevice] = login.ExitsPlayerDevice;
            //syncContext.KeyDictions[SysConstants.OpenScreenRecord] = login.OpenScreenRecord;
            //syncContext.KeyDictions[SysConstants.OpenScreenWall] = login.OpenScreenWall;
            syncContext[SysConstants.IdentifyId] = login.IdentifyId;
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
                var login = session.GetMessageEntity<LoginPacket>();
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

                    if (adapterHandler.IsManualClose())//如果是手动结束任务
                        return;

                    adapterHandler.State = "工作连接已断开,正在重新连接中....";
                    adapterHandler.SessionClosed(session);

                    //非手动结束任务，将该任务扔到重连线程中
                    var appName = adapterHandler.App.GetType().Name;
                    TaskScheduleTrigger.AddScheduleTask(new SuspendTaskContext()
                    {
                        DisconnectTimePoint = DateTime.Now,
                        ApplicationAdapterHandler = adapterHandler,
                        SessionSyncContexts = SessionSyncContexts,
                        TaskName = $"{adapterHandler.IdentifyId},{appName}.{adapterHandler.GetApplicationKey()}"
                    });
                }
                else if (worktype == ConnectionWorkType.MAINCON)
                {
                    var syncContext = arguments[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
                    if (syncContext.IsNull())
                        return;

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
                new RemoteUpdatePacket()
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
                new MessagePacket()
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
        public void RemoteActivateService(SessionSyncContext syncContext, string appKey)
        {
            Thread.Sleep(1000);
            syncContext.Session.SendTo(MessageHead.S_MAIN_ACTIVATE_APPLICATION_SERVICE,
                new ActivateServicePack()
                {
                    CommandText = appKey
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
