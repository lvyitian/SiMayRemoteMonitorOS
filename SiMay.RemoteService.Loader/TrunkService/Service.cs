using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Platform.Windows;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SiMay.RemoteService.Loader
{
    public partial class Service : ServiceBase
    {
        private class UserProcessToken
        {
            public int SessionId { get; set; }
            public bool Actived { get; set; }
            public DateTime LastActiveTime { get; set; }
        }
        public Service()
        {
            InitializeComponent();
        }

        private bool _isRun = true;
        private int _port = 0;
        private List<UserProcessToken> _userProcessSessionIdList = new List<UserProcessToken>();
        private readonly object _lock = new object();
        private PacketModelBinder<TcpSocketSaeaSession, TrunkMessageHead> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession, TrunkMessageHead>();
        protected override void OnStart(string[] args)
        {
            var serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.AppKeepAlive = true;
            serverConfig.PendingConnectionBacklog = 0;
            var trunkService = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, serverConfig,
                (notity, session) =>
            {
                switch (notity)
                {
                    case TcpSessionNotify.OnConnected:
                        break;
                    case TcpSessionNotify.OnSend:
                        break;
                    case TcpSessionNotify.OnDataReceiveing:
                        break;
                    case TcpSessionNotify.OnDataReceived:
                        _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<TrunkMessageHead>(), this);
                        break;
                    case TcpSessionNotify.OnClosed:
                        this.SessionClosedHandler(session);
                        break;
                    default:
                        break;
                }
            });

            bool completed = false;
            for (int trycount = 0; trycount < 100; trycount++)
            {
                try
                {
                    _port = 10000 + trycount;
                    var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
                    trunkService.Listen(ipEndPoint);
                    completed = true;
                    break;
                }
                catch (Exception ex)
                {
                    //LogHelper.WriteErrorByCurrentMethod("trunkService open listen exception:" + ex.Message);
                }
                completed = false;
                Thread.Sleep(1000);
            }
            if (!completed)
            {
                //LogHelper.WriteErrorByCurrentMethod("listen all tcp port not completed，please check!");
                Environment.Exit(0);//监听所有端口失败
            }


            // Obtain session ID for active session.
            uint dwSessionId = Kernel32.WTSGetActiveConsoleSessionId();

            // Check for RDP session.  If active, use that session ID instead.
            var rdpSessionID = Win32Interop.GetRDPSession();
            if (rdpSessionID > 0)
            {
                dwSessionId = rdpSessionID;
            }
            this.CreateProcessAsUser(dwSessionId, true);
            ThreadHelper.CreateThread(() =>
            {
                while (_isRun)
                {
                    lock (_lock)
                        for (int i = 0; i < _userProcessSessionIdList.Count; i++)
                        {
                            var token = _userProcessSessionIdList[i];
                            if (token.Actived)
                                continue;
                            //LogHelper.DebugWriteLog("!Actived:" + token.SessionId);
                            if ((int)(DateTime.Now - token.LastActiveTime).TotalSeconds > 5)//如果用户进程5秒内未重新激活
                            {
                                bool completed = this.CreateProcessAsUser((uint)token.SessionId);//可能用户进程已结束，重新启动用户进程
                                //LogHelper.DebugWriteLog("Restart ProcessAsUser:" + completed);
                                if (!completed && !Win32Interop.EnumerateSessions().Any(c => c.SessionID == token.SessionId))
                                {
                                    _userProcessSessionIdList.RemoveAt(i);//如果重启失败移除会话信息，可能会话已注销
                                    i--;
                                    if (_userProcessSessionIdList.Count == 0)//可能系统已注销
                                    {
                                        var activeSessionId = Kernel32.WTSGetActiveConsoleSessionId();//获取活动会话
                                        var isOk = this.CreateProcessAsUser(activeSessionId, true);
                                        //LogHelper.DebugWriteLog("Restart ProcessAsUser activeSessionId:" + activeSessionId + " status:" + isOk);
                                    }
                                    continue;
                                }
                                token.LastActiveTime = DateTime.Now;//延迟最后时间，给用户进程足够时间激活
                            }
                        }
                    Thread.Sleep(1000);
                }

            }, true);
        }

        private bool CreateProcessAsUser(uint dwSessionId, bool isPush = false)
        {
            var desktopName = Win32Interop.GetCurrentDesktop();
            var openProcessString = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            //用户进程启动
            var result = Win32Interop.OpenInteractiveProcess(openProcessString + $" \"-user\" \"-port:{_port}\" \"-sessionId:{dwSessionId}\"", desktopName, true, dwSessionId, out _);
            if (isPush)
                _userProcessSessionIdList.Add(new UserProcessToken()
                {
                    SessionId = (int)dwSessionId,
                    Actived = false//默认不激活状态，等待用户进程连接以激活
                });

            return result;
        }

        [PacketHandler(TrunkMessageHead.S_Active)]
        private void ActiveHandler(TcpSocketSaeaSession session)
        {
            var sessionId = session.CompletedBuffer.GetMessageEntity<ActivePack>().SessionId;
            //LogHelper.DebugWriteLog("ActiveHandler:" + sessionId);
            session.AppTokens = new object[] {
                sessionId
            };
            lock (_lock)
            {
                var token = _userProcessSessionIdList.Find(c => c.SessionId == sessionId);
                if (token == null)
                    return;
                token.Actived = true;
            }
        }

        private void SessionClosedHandler(TcpSocketSaeaSession session)
        {
            if (session.AppTokens == null && session.AppTokens.Length == 0)
                return;
            //LogHelper.DebugWriteLog("SessionClosedHandler");
            var sessionId = (int)session.AppTokens[0];

            lock (_lock)
            {
                var token = _userProcessSessionIdList.Find(c => c.SessionId == sessionId);
                if (token == null)
                    return;//已主动退出
                token.Actived = false;//当连接意外离线时 
                token.LastActiveTime = DateTime.Now;
                //LogHelper.DebugWriteLog("连接意外断开");
            }
        }

        [PacketHandler(TrunkMessageHead.S_CreateUserProcess)]
        private void CreateUserProcessHandler(TcpSocketSaeaSession session)
        {
            var dwSessionId = session.CompletedBuffer.GetMessageEntity<CreateUserProcessPack>().SessionId;
            lock (_lock)
            {
                if (_userProcessSessionIdList.Any(c => c.SessionId == dwSessionId))
                    return;
                //用户进程启动
                this.CreateProcessAsUser((uint)dwSessionId, true);
            }
        }

        [PacketHandler(TrunkMessageHead.S_SendSas)]
        private void SendSasHandler(TcpSocketSaeaSession session)
        {
            //LogHelper.DebugWriteLog("SendSasHandler");
            User32.SendSAS(false);
        }

        [PacketHandler(TrunkMessageHead.S_InitiativeExit)]
        private void InitiativeExitHandler(TcpSocketSaeaSession session)
        {
            if (session.AppTokens == null && session.AppTokens.Length == 0)
                return;
            var sessionId = (int)session.AppTokens[0];
            lock (_lock)
            {
                var index = _userProcessSessionIdList.FindIndex(c => c.SessionId == sessionId);
                if (index == -1)
                    return;
                _userProcessSessionIdList.RemoveAt(index);

                if (_userProcessSessionIdList.Count <= 0)//主动退出
                {
                    //LogHelper.DebugWriteLog("InitiativeExitHandler");
                    _isRun = false;
                    Environment.Exit(0);
                }
            }
        }
        [PacketHandler(TrunkMessageHead.S_EnumerateSessions)]
        private void EnumerateSessionsHandler(TcpSocketSaeaSession session)
        {
            lock (_lock)
            {
                var sessions = Win32Interop.EnumerateSessions()
                    .Select(c => new SessionItem()
                    {
                        UserName = WTSAPI32.GetUserNameBySessionId(c.SessionID),
                        SessionState = (int)c.State,
                        WindowStationName = c.pWinStationName,
                        SessionId = c.SessionID,
                        HasUserProcess = _userProcessSessionIdList.FindIndex(i => i.SessionId == c.SessionID) > -1 ? true : false
                    })
                    .ToArray();
                //LogHelper.DebugWriteLog("Session-Count:" + sessions.Count());
                var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.C_SessionItems,
                    new SessionStatusPack()
                    {
                        Sessions = sessions
                    });
                session.SendAsync(data);
            }
        }
        protected override void OnStop()
        {
            try
            {
                var process = Process.GetProcesses();
                foreach (Process pro in process)
                {
                    if (pro.ProcessName.Equals(Process.GetCurrentProcess().ProcessName, StringComparison.OrdinalIgnoreCase) && pro.Id != Process.GetCurrentProcess().Id)
                    {
                        pro.Kill();
                    }
                }
            }
            catch { }
        }
    }
}
