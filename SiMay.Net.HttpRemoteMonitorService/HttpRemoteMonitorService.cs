using Newtonsoft.Json;
using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.Packets;
using SiMay.Net.HttpRemoteMonitorService.HttpModel;
using SiMay.Net.HttpRemoteMonitorService.HttpPackageModel;
using SiMay.Net.HttpRemoteMonitorService.Properties;
using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.Serialize;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.HttpRemoteMonitorService
{
    public class HttpRemoteMonitorService
    {
        SessionProvider.SessionProvider _sessionProvider;

        WebSocketSession _manager_session;
        int _manager_login_c = 0;
        bool _logOut = false;

        string _desktopView_width = "0";
        string _desktopView_height = "0";
        string _desktopView_spantime = "100";

        string _login_id = AppConfiguration.LoginId;
        string _login_password = AppConfiguration.LoginPassWord;

        Dictionary<string, SessionHandler> _sessionDictionary = new Dictionary<string, SessionHandler>();

        Queue<Consolelog> _logQueue = new Queue<Consolelog>(10);

        public HttpRemoteMonitorService()
        {
            InitService();
            Thread log_thread = new Thread(ShowLogWorkThread);
            log_thread.IsBackground = true;
            log_thread.Start();
        }

        private void ShowLogWorkThread()
        {
            while (true)
            {
                if (_logQueue.Count > 0)
                {
                    var log = _logQueue.Dequeue();
                    this.ConsoleWriteLine(log.Log, log.ConsoleColor);
                }
                Thread.Sleep(50);
            }
        }

        private void ConsoleWriteLine(string log, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(DateTime.Now + Environment.NewLine + log);
            Console.ResetColor();
        }

        private void PutLogQueue(string log,ConsoleColor color = ConsoleColor.White)
        {
            _logQueue.Enqueue(new Consolelog()
            {
                Log = log,
                ConsoleColor = color
            });
        }
        private void InitService()
        {
            string localIPAddress = AppConfiguration.ServiceIPAddress;
            int port = int.Parse(AppConfiguration.ServicePort);

            Console.Title = "Http远程控制服务 BEAT " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.PutLogQueue("DEBUG localIPAddress:" + localIPAddress);
            this.PutLogQueue("DEBUG port:" + port);

            WebSocketServer server = new WebSocketServer();
            server.NewSessionConnected += Server_NewSessionConnected;
            server.NewMessageReceived += Server_NewMessageReceived;
            server.SessionClosed += Server_SessionClosed;
            server.Setup(localIPAddress, port);
            server.Start();

            string serviceIPAddress = AppConfiguration.SessionServiceIPAddress;
            int servicePort = int.Parse(AppConfiguration.SessionServicePort);
            long accessKey = int.Parse(AppConfiguration.AccessKey);

            this.PutLogQueue("DEBUG serviceIPAddress:" + serviceIPAddress);
            this.PutLogQueue("DEBUG servicePort:" + servicePort);
            this.PutLogQueue("DEBUG accessKey:" + accessKey);
            this.PutLogQueue("DEBUG id:" + _login_id);
            this.PutLogQueue("DEBUG password:" + _login_password);

            var ipe = new IPEndPoint(IPAddress.Parse(serviceIPAddress), servicePort);
            SessionProviderOptions options = new SessionProviderOptions()
            {
                ServiceIPEndPoint = ipe,
                PendingConnectionBacklog = 0,
                AccessKey = accessKey
            };

            options.SessionProviderType = SessionProviderType.TcpProxySession;

            _sessionProvider = SessionProviderFactory.CreateProxySessionProvider(options, OnNotifyProc, OnProxyNotify);

            _sessionProvider.StartSerivce();

        }

        private void OnProxyNotify(ProxyNotify notify)
        {
            switch (notify)
            {
                case ProxyNotify.AccessKeyWrong:
                    break;
                case ProxyNotify.LogOut:
                    LogOut();
                    break;
            }
        }

        private void LogOut()
        {
            this.PutLogQueue("ManagerChannel LogOut", ConsoleColor.Yellow);
            _logOut = true;

            if (this._manager_login_c == 0) return;

            ManagerLogOutModel model = new ManagerLogOutModel()
            {
                Msg = AJaxMsgCommand.S_MANAGERCHANNEL_LOGOUT
            };

            this.SendMessage(JsonConvert.SerializeObject(model));
        }

        private void OnNotifyProc(SessionCompletedNotify notify, SessionHandler session)
        {
            switch (notify)
            {
                case SessionCompletedNotify.OnConnected:
                    this.InitSessionAppToken(session);
                    break;
                case SessionCompletedNotify.OnSend:
                    break;
                case SessionCompletedNotify.OnRecv:
                    break;
                case SessionCompletedNotify.OnReceived:
                    this.OnMessage(session);
                    break;
                case SessionCompletedNotify.OnClosed:
                    this.SessionClosed(session);
                    break;
            }
        }

        private void OnMessage(SessionHandler session)
        {
            object[] ars = session.AppTokens;
            ConnectionWorkType sessionWorkType = (ConnectionWorkType)ars[0];

            if (sessionWorkType == ConnectionWorkType.WORKCON)
            {
                //消息扔给消息提供器,由提供器提供消息给所在窗口进行消息处理
                //((MessageAdapter)ars[1]).OnSessionNotify(session, SessionNotifyType.Message);
            }
            else if (sessionWorkType == ConnectionWorkType.MAINCON)
            {
                MessageHead cmd = session.CompletedBuffer.GetMessageHead<MessageHead>();
                switch (cmd)
                {
                    case MessageHead.C_MAIN_LOGIN://上线包
                        this.ProcessLogin(session);
                        break;
                    case MessageHead.C_MAIN_SCREENWALL_IMG://屏幕视图数据
                        this.ProcessDesktopViewData(session, session.CompletedBuffer.GetMessagePayload());
                        break;
                }
            }
            else if (sessionWorkType == ConnectionWorkType.NONE) //初始消息只能进入该方法块处理，连接密码验证正确才能正式处理消息
            {
                switch (session.CompletedBuffer.GetMessageHead<MessageHead>())
                {
                    case MessageHead.C_GLOBAL_CONNECT://连接确认包
                                                     //CheckSessionValidity(session);

                        session.AppTokens[0] = ConnectionWorkType.MAINCON;

                        byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_GLOBAL_OK);
                        session.SendAsync(data);

                        break;
                    default:
                        session.SessionClose();//伪造包断掉
                        break;
                }
            }
        }

        private void ProcessDesktopViewData(SessionHandler session, byte[] bytes)
        {
            if (this._manager_login_c == 0) return;

            string id = (string)session.AppTokens[1];

            DesktopViewModel model = new DesktopViewModel()
            {
                Msg = AJaxMsgCommand.S_DESKTOPVIEW_IMG,
                Id = id,
                ImageBase64 = Convert.ToBase64String(bytes)
            };

            this.SendMessage(JsonConvert.SerializeObject(model));
            this.PutLogQueue("DEBUG ImageProcess id:" + id + " " + bytes.Length / 1024 + "k");
        }

        private void SessionClosed(SessionHandler session)
        {
            string id = (string)session.AppTokens[1];
            this._sessionDictionary.Remove(id);

            if (this._manager_login_c == 0) return;

            object[] arguments = session.AppTokens;
            ConnectionWorkType Worktype = (ConnectionWorkType)arguments[0];
            if (Worktype == ConnectionWorkType.MAINCON)
            {
                SessionCloseModel model = new SessionCloseModel()
                {
                    Msg = AJaxMsgCommand.S_SESSION_CLOSE,
                    Id = id
                };
                this.SendMessage(JsonConvert.SerializeObject(model));

                this.PutLogQueue("DEBUG Session Closed id:" + id);
            }
        }

        private void InitSessionAppToken(SessionHandler session)
        {
            string id = "A" + Guid.NewGuid().ToString().Replace("-", "");
            //分配NONE,等待工作指令分配新的工作类型
            session.AppTokens = new object[3]
            {
                ConnectionWorkType.NONE,
                id,
                false//屏幕视图打开状态
            };

            this._sessionDictionary.Add(id, session);
            this.PutLogQueue("DEBUG Session Connect id:" + id);
        }

        private void ProcessLogin(SessionHandler session)
        {
            LoginPack login = PacketSerializeHelper.DeserializePacket<LoginPack>(session.CompletedBuffer.GetMessagePayload());


            session.AppTokens[2] = login.OpenScreenWall;

            if (this._manager_login_c == 0) return;

            LoginPackageModel login_model = new LoginPackageModel()
            {
                Msg = AJaxMsgCommand.S_SESSION_LOGIN,
                Id = (string)session.AppTokens[1],
                OS = login.OSVersion,
                MachineName = login.MachineName,
                Des = login.Remark,
                DesktopViewOpen = login.OpenScreenWall.ToString().ToLower()
            };


            this.SendMessage(JsonConvert.SerializeObject(login_model));
        }

        private void Server_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            if (session.Items.ContainsKey(1))
                Interlocked.Decrement(ref _manager_login_c);

            session.Items.Clear();

            this.PutLogQueue("Websocket Session Closed!", ConsoleColor.DarkYellow);
        }
        private void Server_NewMessageReceived(WebSocketSession session, string value)
        {
            try
            {
                dynamic request = JsonConvert.DeserializeObject(value);
                AJaxMsgCommand cmd = (AJaxMsgCommand)request.msg;
                switch (cmd)
                {
                    case AJaxMsgCommand.B_LOGIN:
                        string login_id = request.id;
                        string password = request.pwd;
                        if (login_id == _login_id && password == _login_password)
                        {
                            this.Login(session);
                        }
                        else
                        {
                            KeyWrongModel model = new KeyWrongModel()
                            {
                                Msg = AJaxMsgCommand.S_IDORKEYWRONG
                            };
                            session.Send(JsonConvert.SerializeObject(model));
                            this.PutLogQueue("IdOrPassword Wrong Id:" + login_id + " Password:" + password, ConsoleColor.Gray);
                        }
                        break;
                    case AJaxMsgCommand.B_DESKTOPVIEW_PULL:

                        _desktopView_width = request.width;
                        _desktopView_height = request.height;

                        string id = request.Id;
                        this.PullDesktopView(id);
                        break;
                    case AJaxMsgCommand.B_RESET_REMARK:
                        this.SetRemark(request);
                        break;
                    case AJaxMsgCommand.B_OPEN_DESKTOPVIEW:
                        this.OpenDesktopView(request);
                        break;
                    case AJaxMsgCommand.B_CLOSE_DESKTOPVIEW:
                        this.CloseDesktopView(request);
                        break;
                    case AJaxMsgCommand.B_MESSAGEBOX:
                        this.ShowMessageBox(request);
                        break;
                    case AJaxMsgCommand.B_SESSION_MANAGER:
                        this.SessionManager(request);
                        break;
                    case AJaxMsgCommand.B_OPEN_URL:
                        this.OpenURL(request);
                        break;
                    case AJaxMsgCommand.B_DOWNLOAD_EX:
                        this.DownloadURL(request);
                        break;
                    case AJaxMsgCommand.B_UNSTALLER:
                        this.Unstaller(request);
                        break;
                }
            }
            catch (Exception e)
            {
                this.PutLogQueue("WebSocketReceived Exception:" + e.Message + Environment.NewLine + "堆栈:" + e.StackTrace);
            }

        }

        private void Login(WebSocketSession session)
        {
            if (this._manager_login_c > 0)
            {
                //登出
                LogOutModel model = new LogOutModel()
                {
                    Msg = AJaxMsgCommand.S_MANAGER_LOGOUT
                };
                this.SendMessage(JsonConvert.SerializeObject(model));
                this.PutLogQueue("Web LogOut!", ConsoleColor.Yellow);
            }

            //标识通过验证

            this._manager_session = session;
            session.Items.Add(1, 1);
            Interlocked.Increment(ref _manager_login_c);

            //重新连接
            if (this._logOut)
            {
                this._sessionProvider.StartSerivce();
                this._logOut = false;
            }

            GetClientLoginInfo();
        }

        private void Server_NewSessionConnected(WebSocketSession session)
        {
            this.PutLogQueue("DEBUG Websocket Session Connect");
        }

        private void OpenDesktopView(dynamic request)
        {
            string id = request.Id;
            if (this._sessionDictionary.ContainsKey(id))
            {
                SessionHandler session = this._sessionDictionary[id];
                if (!(bool)session.AppTokens[2])
                {
                    byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_DESKTOPVIEW);
                    session.SendAsync(data);

                    session.AppTokens[2] = true;
                    this.PullDesktopView(id);
                }
            }
        }
        private void CloseDesktopView(dynamic request)
        {
            string id = request.Id;
            if (this._sessionDictionary.ContainsKey(id))
            {
                SessionHandler session = this._sessionDictionary[id];
                if ((bool)session.AppTokens[2])
                {
                    session.AppTokens[2] = false;

                    byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_USERDESKTOP_CLOSE);
                    session.SendAsync(data);
                }
            }
        }
        private void Unstaller(dynamic request)
        {
            string id = request.Id;

            if (this._sessionDictionary.ContainsKey(id))
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SESSION, new byte[] { 6 });
                this._sessionDictionary[id].SendAsync(data);
            }

        }
        private void DownloadURL(dynamic request)
        {
            string id = request.Id;
            string url = request.val;

            if (this._sessionDictionary.ContainsKey(id))
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_HTTPDOWNLOAD, url);
                this._sessionDictionary[id].SendAsync(data);
            }
        }
        private void OpenURL(dynamic request)
        {
            string id = request.Id;
            string url = request.val;

            if (this._sessionDictionary.ContainsKey(id))
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_OPEN_WEBURL, url);
                this._sessionDictionary[id].SendAsync(data);
            }
        }
        private void SessionManager(dynamic request)
        {
            string id = request.Id;
            string val = request.val;

            if (this._sessionDictionary.ContainsKey(id))
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SESSION, new byte[] { byte.Parse(val) });
                this._sessionDictionary[id].SendAsync(data);
            }
        }
        private void ShowMessageBox(dynamic request)
        {
            string id = request.Id;
            string body = request.val;
            if (this._sessionDictionary.ContainsKey(id))
            {
                MessagePack msg = new MessagePack();
                msg.MessageIcon = (byte)MessageIcon.InforMation;
                msg.MessageTitle = "系统通知";
                msg.MessageBody = body;

                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_MESSAGEBOX, PacketSerializeHelper.SerializePacket(msg));
                this._sessionDictionary[id].SendAsync(data);
            }
        }

        private void SetRemark(dynamic request)
        {
            string id = request.Id;
            string des = request.val;
            if (this._sessionDictionary.ContainsKey(id))
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_REMARK, des);
                this._sessionDictionary[id].SendAsync(data);
            }
        }
        /// <summary>
        /// 获取桌面视图
        /// </summary>
        /// <param name="id"></param>
        private void PullDesktopView(string id)
        {
            if (this._sessionDictionary.ContainsKey(id))
            {
                SessionHandler session = this._sessionDictionary[id];

                if (!(bool)session.AppTokens[2])
                    return;

                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SCREENWALL_GETIMG, _desktopView_width + "|" + _desktopView_height + "|" + _desktopView_spantime);
                session.SendAsync(data);

                this.PutLogQueue("DEBUG pull Image height:" + _desktopView_height + " width:" + _desktopView_width);
            }
        }

        /// <summary>
        /// 获取上线信息
        /// </summary>
        private void GetClientLoginInfo()
        {
            foreach (var item in this._sessionDictionary)
            {
                byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_GLOBAL_OK);
                item.Value.SendAsync(data);
            }
        }

        private void SendMessage(string msg)
        {
            try
            {
                this._manager_session.Send(msg);
            }
            catch (Exception e)
            {
                this._manager_session.Close();
                this.PutLogQueue("SendMessage ExceptionMessage:" + e.Message);
            }

        }
    }
}