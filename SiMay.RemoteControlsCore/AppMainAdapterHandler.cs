using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Delegate;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Net.SessionProvider.SessionBased;
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
    public class AppMainAdapterHandler
    {
        /// <summary>
        /// 当有数据上传时
        /// </summary>
        public event Action<SessionHandler> OnTransmitHandlerEvent;

        /// <summary>
        /// 当正在接收数据时
        /// </summary>

        public event Action<SessionHandler> OnReceiveHandlerEvent;

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
        /// 创建屏幕视图处理事件
        /// </summary>
        public event Func<SessionSyncContext, IDesktopView> OnCreateDesktopViewHandlerEvent;

        /// <summary>
        /// 代理协议事件
        /// </summary>
        public event OnProxyNotify<ProxyNotify> OnProxyNotifyHandlerEvent;

        /// <summary>
        /// 监听日志事件
        /// </summary>
        public event Action<string, LogSeverityLevel> OnLogHandlerEvent;

        /// <summary>
        /// 视图墙刷新间隔
        /// </summary>
        public int ViewRefreshInterval { get; set; }
        /// <summary>
        /// 线程同步上下文
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; set; }

        /// <summary>
        /// 会话提供器
        /// </summary>
        public SessionProvider SessionProvider { get; set; }

        /// <summary>
        /// 主连接同步上下文对象集合
        /// </summary>
        public List<SessionSyncContext> SyncContexts { get; set; } = new List<SessionSyncContext>();

        /// <summary>
        /// 离线重连池
        /// </summary>
        private ResetPool _resetPool;
        private PacketModelBinder<SessionHandler, MessageHead> _handlerBinder = new PacketModelBinder<SessionHandler, MessageHead>();

        public void StartService()
        {
            this._resetPool = new ResetPool(SyncContexts);

            if (!int.TryParse(AppConfiguration.SessionMode, out var sessionMode))
                sessionMode = 0;

            string ip = sessionMode == 0
                ? AppConfiguration.IPAddress
                : AppConfiguration.ServiceIPAddress;

            int port = sessionMode == 0 ? AppConfiguration.Port : AppConfiguration.ServicePort;

            int maxconnectCount = AppConfiguration.MaxConnectCount;

            var options = new SessionProviderOptions
            {
                ServiceIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port),
                PendingConnectionBacklog = maxconnectCount,
                AccessKey = long.Parse(AppConfiguration.AccessKey)
            };

            var providerType = int.Parse(AppConfiguration.SessionMode).ConvertTo<SessionProviderType>();
            options.SessionProviderType = providerType;
            if (providerType == SessionProviderType.TcpServiceSession)
            {
                if (StartServiceProvider(options))
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统端口 {port.ToString()} 监听成功!", LogSeverityLevel.Information);
                else
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统端口 {port.ToString()} 启动失败,请检查配置!", LogSeverityLevel.Warning);
            }
            else
            {
                if (StartProxySessionProvider(options))
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统初始化成功!", LogSeverityLevel.Information);
                else
                    this.OnLogHandlerEvent?.Invoke($"SiMay远程监控管理系统初始化发生错误，请注意检查配置!", LogSeverityLevel.Warning);
            }

            bool StartServiceProvider(SessionProviderOptions providerOptions)
            {
                SessionProvider = SessionProviderFactory.CreateTcpSessionProvider(providerOptions, OnNotifyProc);
                try
                {
                    SessionProvider.StartSerivce();
                    return true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return false;
                }
            }

            bool StartProxySessionProvider(SessionProviderOptions providerOptions)
            {
                SessionProvider = SessionProviderFactory.CreateProxySessionProvider(options, OnNotifyProc, OnProxyNotifyHandlerEvent);
                try
                {
                    SessionProvider.StartSerivce();
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
        private void OnNotifyProc(SessionCompletedNotify notify, SessionHandler session)
        {
            if (SynchronizationContext == null)
                NotifyProc(null);
            else
                SynchronizationContext.Send(NotifyProc, null);

            void NotifyProc(object val)
            {
                try
                {
                    switch (notify)
                    {
                        case SessionCompletedNotify.OnConnected:
                            //先分配好工作类型，等待工作指令分配新的工作类型
                            session.AppTokens = new object[SysConstants.INDEX_COUNT]
                            {
                                ConnectionWorkType.NONE,//未经验证的状态
                                null
                            };
                            break;
                        case SessionCompletedNotify.OnSend:
                            //耗时操作会导致性能严重降低
                            this.OnTransmitHandlerEvent?.Invoke(session);
                            break;
                        case SessionCompletedNotify.OnRecv:
                            //耗时操作会导致性能严重降低
                            this.OnReceiveHandlerEvent?.Invoke(session);
                            break;
                        case SessionCompletedNotify.OnReceived:
                            this.OnReceiveComplete(session);
                            break;
                        case SessionCompletedNotify.OnClosed:
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

        private void OnReceiveComplete(SessionHandler session)
        {
            // Tokens参数说明
            // [0]为该连接工作类型，MainWork为主连接，Work工作连接，NONE为未知连接
            // [1]如果连接为Work类型，则是消息处理器，否则是主连接上下文对象
            var appTokens = session.AppTokens;
            var sessionWorkType = appTokens[SysConstants.INDEX_WORKTYPE].ConvertTo<ConnectionWorkType>();

            if (sessionWorkType == ConnectionWorkType.WORKCON)
            {
                //消息传给消息处理器,由消息处理器所在App进行处理
                var app = appTokens[SysConstants.INDEX_WORKER].ConvertTo<AdapterHandlerBase>();
                if (app.IsClose)
                    return;
                app.HandlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), app);
            }
            else if (sessionWorkType == ConnectionWorkType.MAINCON)
                _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), this);
            else if (sessionWorkType == ConnectionWorkType.NONE) //未经过验证的连接的消息只能进入该方法块处理，连接密码验证正确才能正式处理消息
            {
                switch (session.CompletedBuffer.GetMessageHead<MessageHead>())
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
        private void ValiditySession(SessionHandler session)
        {
            long accessKey = BitConverter.ToInt64(session.CompletedBuffer.GetMessagePayload(), 0);
            if (accessKey != int.Parse(AppConfiguration.ConnectPassWord))
            {
                session.SessionClose();
                return;
            }
            else
            {
                //连接密码验证通过，设置成为主连接，正式接收处理数据包
                session.AppTokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.MAINCON;

                //告诉服务端一切就绪
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_GLOBAL_OK);
                session.SendAsync(data);
            }
        }

        /// <summary>
        /// 启动App
        /// </summary>
        /// <param name="type"></param>
        /// <param name="session"></param>
        /// <param name="identifyId"></param>
        [PacketHandler(MessageHead.C_MAIN_ACTIVE_APP)]
        private void OnActivateStartApp(SessionHandler session)
        {
            var openControl = session.CompletedBuffer.GetMessageEntity<ActiveAppPack>();
            string originName = openControl.OriginName;
            string appKey = openControl.ServiceKey;
            string identifyId = openControl.IdentifyId;
            //查找离线任务队列,如果有对应的任务则继续工作
            var task = _resetPool.FindTask(identifyId);
            if (task != null)
            {
                //再发出重连命令后，如果使用者主动关闭消息处理器将不再建立连接
                if (task.AdapterHandler.IsClose)
                {
                    //通知远程释放资源
                    byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_GLOBAL_ONCLOSE);
                    session.SendAsync(data);
                    return;
                }

                //将消息处理器与会话关联
                var tokens = session.AppTokens;
                tokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.WORKCON;
                tokens[SysConstants.INDEX_WORKER] = task.AdapterHandler;
                task.AdapterHandler.Session = session;
                task.AdapterHandler.ContinueTask(session);//继续任务

                _resetPool.RemoveTask(identifyId);
            }
            else
            {
                var context = SysUtil.ApplicationTypes.FirstOrDefault(x => x.ApplicationKey.Equals(appKey));
                if (context != null)
                {

                    var appHandlerType = context.Type.GetAppAdapterHandlerType();
                    AdapterHandlerBase appHandlerBase = Activator.CreateInstance(appHandlerType).ConvertTo<AdapterHandlerBase>();
                    IApplication app = Activator.CreateInstance(context.Type).ConvertTo<IApplication>();

                    appHandlerBase.App = app;
                    appHandlerBase.Session = session;
                    appHandlerBase.IdentifyId = identifyId;
                    appHandlerBase.OriginName = originName;
                    appHandlerBase.ResetApplicationKey = context.Type.GetAppKey();

                    //每个应用至少标记一个应用处理器属性
                    var handlerFieder = context
                        .Type
                        .GetProperties()
                        .Single(c => c.GetCustomAttribute<ApplicationAdapterHandlerAttribute>(true) != null);
                    handlerFieder.SetValue(app, appHandlerBase);

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


        /// <summary>
        /// 创建桌面记录任务
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_SCREEN_RECORD_OPEN)]
        private void StartScreenRecordTaskHandler(SessionHandler session)
        {
            string macName = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
            syncContext.KeyDictions[SysConstants.RecordScreenIsAction] = true;//开启
            syncContext.KeyDictions[SysConstants.MachineName] = macName;//标识名(用计算机名作为唯一标识)

            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SCREEN_RECORD_GETIMG);
            session.SendAsync(data);
        }

        /// <summary>
        /// 储存桌面记录信息
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_SCREEN_RECORD_IMG)]
        private void ScreenSaveHandler(SessionHandler session)
        {
            var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
            bool status = syncContext.KeyDictions[SysConstants.RecordScreenIsAction].ConvertTo<bool>();
            string macName = syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>();

            if (!Directory.Exists(Path.Combine("ScreenRecord", macName)))
                Directory.CreateDirectory(Path.Combine("ScreenRecord", macName));

            using (var ms = new MemoryStream(session.CompletedBuffer.GetMessagePayload()))
            {
                string fileName = Path.Combine(Environment.CurrentDirectory, "ScreenRecord", macName, DateTime.Now.ToFileTime() + ".png");
                Image img = Image.FromStream(ms);
                img.Save(fileName);
                img.Dispose();
            }

            if (!status)
                return;

            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SCREEN_RECORD_GETIMG);
            session.SendAsync(data);
        }

        /// <summary>
        /// 创建桌面视图
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_USERDESKTOP_CREATE)]
        private void OnCreateDesktopView(SessionHandler session)
        {
            var describePack = session.CompletedBuffer.GetMessageEntity<DesktopViewDescribePack>();
            var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
            var view = this.OnCreateDesktopViewHandlerEvent?.Invoke(syncContext);
            if (view == null)
                return;
            view.Caption = describePack.MachineName + "-(" + describePack.RemarkInformation + ")";
            syncContext.KeyDictions[SysConstants.DesktopView] = view;
            this.GetViewFrame(session, view);
        }


        /// <summary>
        /// 显示桌面墙数据
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_SCREENWALL_IMG)]
        private void PlayerDesktopImage(SessionHandler session)
        {
            var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
            if (!syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView) ||
                syncContext.KeyDictions[SysConstants.DesktopView] == null)
                return;

            var frameData = session.CompletedBuffer.GetMessageEntity<DesktopViewFramePack>();
            var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
            if (frameData.InVisbleArea)
            {
                using (var ms = new MemoryStream(frameData.ViewData))
                    view.PlayerDekstopView(Image.FromStream(ms));
            }
            this.GetViewFrame(session, view);
        }

        private void GetViewFrame(SessionHandler session, IDesktopView view)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(
                MessageHead.S_MAIN_SCREENWALL_GETIMG, new DesktopViewGetFramePack()
                {
                    Height = view.Height,
                    Width = view.Width,
                    TimeSpan = this.ViewRefreshInterval,
                    InVisbleArea = view.InVisbleArea
                });

            session.SendAsync(data);
        }

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
            syncContext.KeyDictions[SysConstants.OpenScreenRecord] = login.OpenScreenRecord;
            syncContext.KeyDictions[SysConstants.OpenScreenWall] = login.OpenScreenWall;
            syncContext.KeyDictions[SysConstants.IdentifyId] = login.IdentifyId;
            syncContext.KeyDictions[SysConstants.RecordScreenIsAction] = false;//桌面记录状态
            syncContext.KeyDictions[SysConstants.RecordHeight] = login.RecordHeight;//用于桌面记录的高
            syncContext.KeyDictions[SysConstants.RecordWidth] = login.RecordWidth;//用于桌面记录宽
            syncContext.KeyDictions[SysConstants.RecordSpanTime] = login.RecordSpanTime;
            syncContext.KeyDictions[SysConstants.HasLoadServiceCOM] = login.HasLoadServiceCOM;

            this.OnLoginUpdateHandlerEvent?.Invoke(syncContext);
        }

        /// <summary>
        /// 添加用户信息到上线列表，并根据用户信息无人值守打开任务
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_MAIN_LOGIN)]
        private void LoginHandler(SessionHandler session)
        {
            try
            {
                var login = session.CompletedBuffer.GetMessageEntity<LoginPack>();
                if (session.AppTokens[SysConstants.INDEX_WORKER] != null)//如果主连接同步对象存在，则对该对象更新
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
                    { SysConstants.OpenScreenWall, login.OpenScreenWall },
                    { SysConstants.IdentifyId, login.IdentifyId },
                    { SysConstants.OpenScreenRecord, login.OpenScreenRecord },
                    { SysConstants.RecordScreenIsAction, false },
                    { SysConstants.RecordHeight, login.RecordHeight },
                    { SysConstants.RecordWidth, login.RecordWidth },
                    { SysConstants.RecordSpanTime, login.RecordSpanTime },
                    { SysConstants.HasLoadServiceCOM, login.HasLoadServiceCOM }
                };
                var syncContext = new SessionSyncContext(session, dictions);
                SyncContexts.Add(syncContext);
                session.AppTokens[SysConstants.INDEX_WORKER] = syncContext;

                //是否开启桌面视图
                if (syncContext.KeyDictions[SysConstants.OpenScreenWall].ConvertTo<bool>())
                {
                    byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_DESKTOPVIEW, new byte[] { 0 });//强制创建视图
                    session.SendMessageDoHasCOM(data);
                }

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
        private void OnClosed(SessionHandler session)
        {
            try
            {
                object[] arguments = session.AppTokens;
                var worktype = arguments[SysConstants.INDEX_WORKTYPE].ConvertTo<ConnectionWorkType>();
                if (worktype == ConnectionWorkType.WORKCON)
                {
                    var adapterHandler = arguments[SysConstants.INDEX_WORKER].ConvertTo<AdapterHandlerBase>();

                    if (adapterHandler.IsClose)//如果是手动结束任务
                        return;

                    adapterHandler.StateContext = "工作连接已断开,正在重新连接中....";
                    adapterHandler.SessionClosed(session);
                    //非手动结束任务，将该任务扔到重连队列中
                    _resetPool.Put(new SuspendTaskContext()
                    {
                        DisconnectTime = DateTime.Now,
                        AdapterHandler = adapterHandler
                    });
                }
                else if (worktype == ConnectionWorkType.MAINCON)
                {
                    var syncContext = arguments[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
                    SyncContexts.Remove(syncContext);

                    if (syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView) && syncContext.KeyDictions[SysConstants.DesktopView] != null)
                    {
                        var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
                        view.CloseDesktopView();
                    }

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
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_RELOADER);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        /// <param name="syncContext"></param>
        public void InstallAutoStartService(SessionSyncContext syncContext)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_INSTANLL_SERVICE);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 卸载自动启动服务
        /// </summary>
        /// <param name="syncContext"></param>
        public void UnInStallAutoStartService(SessionSyncContext syncContext)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_UNINSTANLL_SERVICE);
            syncContext.Session.SendMessageDoHasCOM(data);
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
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_UPDATE,
                new RemoteUpdatePack()
                {
                    UrlOrFileUpdate = updateType,
                    DownloadUrl = updateType == RemoteUpdateType.Url ? url : string.Empty,
                    FileDate = updateType == RemoteUpdateType.File ? file : new byte[0]
                });
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 设置分组
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="groupName"></param>
        public void RemoteSetGroupName(SessionSyncContext syncContext, string groupName)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_GROUP, groupName);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 打开桌面视图
        /// </summary>
        /// <param name="syncContext"></param>
        public void RemoteOpenDesktopView(SessionSyncContext syncContext)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_DESKTOPVIEW, new byte[] { 1 });
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 关闭桌面视图
        /// </summary>
        /// <param name="syncContext"></param>
        public void RemoteCloseDesktopView(SessionSyncContext syncContext)
        {
            if (!syncContext.KeyDictions.ContainsKey(SysConstants.DesktopView))
                return;
            var view = syncContext.KeyDictions[SysConstants.DesktopView].ConvertTo<IDesktopView>();
            view.CloseDesktopView();

            syncContext.KeyDictions.Remove(SysConstants.DesktopView);

            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_USERDESKTOP_CLOSE);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 远程打开Url
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="url"></param>
        public void RemoteOpenUrl(SessionSyncContext syncContext, string url)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_OPEN_WEBURL,
                url);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 设置系统会话状态
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="sessionType"></param>
        public void RemoteSetSessionState(SessionSyncContext syncContext, SystemSessionType sessionType)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SESSION, new byte[] { (byte)sessionType });
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 远程下载执行
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="url"></param>
        public void RemoteHttpDownloadExecute(SessionSyncContext syncContext, string url)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_HTTPDOWNLOAD,
                url);
            syncContext.Session.SendMessageDoHasCOM(data);
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
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_MESSAGEBOX,
                new MessagePack()
                {
                    MessageTitle = title,
                    MessageBody = text,
                    MessageIcon = (byte)icon
                });

            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 修改远程备注
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="remark"></param>
        public void RemoteSetRemarkInformation(SessionSyncContext syncContext, string remark)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_REMARK, remark);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        /// <summary>
        /// 开打远程应用服务
        /// </summary>
        /// <param name="syncContext"></param>
        /// <param name="appKey"></param>
        public void RemoteActiveService(SessionSyncContext syncContext, string appKey)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_ACTIVATE_CTRLSERVICE, appKey);
            syncContext.Session.SendMessageDoHasCOM(data);
        }

        public void CloseService()
        {
            SessionProvider.CloseService();
            this._handlerBinder.Dispose();
            this._resetPool.Stop();
        }
    }
}
