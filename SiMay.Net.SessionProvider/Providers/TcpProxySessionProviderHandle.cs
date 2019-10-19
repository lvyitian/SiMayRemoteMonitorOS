using SiMay.Net.SessionProvider.Delegate;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Server;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SiMay.Net.SessionProvider.Core;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Basic;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;

namespace SiMay.Net.SessionProvider.Providers
{
    /// <summary>
    /// 代理协议写的有些冗余。。有空抽出时间重构
    /// </summary>
    public class TcpProxySessionProviderHandle : SessionProvider
    {
        const Int16 AckPacket = 1000;

        List<TcpProxySessionBased> _sessionList = new List<TcpProxySessionBased>();

        TcpSocketSaeaClientAgent _clientAgent;
        SessionProviderOptions _options;

        List<byte> _manager_buffer = new List<byte>();
        TcpSocketSaeaSession _manager_session;
        int _manager_login = 0;
        bool _LogOut = false;

        OnProxyNotify<ProxyNotify> _onProxyNotify;
        internal TcpProxySessionProviderHandle(
            SessionProviderOptions options,
            OnSessionNotify<SessionCompletedNotify, SessionHandler> onSessionNotifyProc,
            OnProxyNotify<ProxyNotify> onProxyNotify)
            : base(onSessionNotifyProc)
        {
            _options = options;
            _onProxyNotify = onProxyNotify;

            var clientConfig = new TcpSocketSaeaClientConfiguration();
            clientConfig.ReuseAddress = true;
            clientConfig.KeepAlive = true;
            clientConfig.KeepAliveInterval = 5000;
            clientConfig.KeepAliveSpanTime = 1000;

            _clientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Full, clientConfig, (notify, session) =>
            {

                switch (notify)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        this.ConnectionProcess(session);
                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        this.OnSend(session);
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        this.PacketProcess(session);
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        this.OnClosed(session);
                        break;
                    default:
                        break;
                }
            });
        }

        private void ConnectionProcess(TcpSocketSaeaSession session)
        {
            if (Interlocked.Exchange(ref _manager_login, 1) == 0)
            {
                this.SendAck(session, SessionWorkType.ManagerSession);

                _manager_session = session;
                session.AppTokens = new object[]
                {
                    SessionWorkType.ManagerSession,
                    null
                };

                //获取所有主连接
                SendMessageHelper.SendMessage(session, MsgCommand.Msg_Pull_Session);
            }
            else
            {
                this.SendAck(session, SessionWorkType.ManagerWorkSession);
                var sessionBased = new TcpProxySessionBased(session);
                session.AppTokens = new object[]
                {
                    SessionWorkType.ManagerWorkSession,
                    sessionBased
                };

                this._onSessionNotifyProc(SessionCompletedNotify.OnConnected, sessionBased as SessionHandler);
            }
        }
        private void OnSend(TcpSocketSaeaSession session)
        {
            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerWorkSession)
            {
                var sessionBased = session.AppTokens[1] as TcpProxySessionBased;
                sessionBased._sendTransferredBytes = session.SendTransferredBytes;
                this._onSessionNotifyProc(SessionCompletedNotify.OnSend, sessionBased as SessionHandler);
            }
        }

        private void PacketProcess(TcpSocketSaeaSession session)
        {
            byte[] data = new byte[session.ReceiveBytesTransferred];
            Array.Copy(session.CompletedBuffer, 0, data, 0, data.Length);

            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                _manager_buffer.AddRange(data);
                do
                {
                    if (_manager_buffer.Count < 4)
                        return;

                    byte[] lenBytes = _manager_buffer.GetRange(0, 4).ToArray();
                    int packageLen = BitConverter.ToInt32(lenBytes, 0);

                    //缓冲区越界判断
                    if (packageLen > (1024 * 1024 * 2) || packageLen < 1)
                    {
                        this.ConsoleWriteLine("DEBUG BUFFER OutOfRange !! PacketProcess MAIN", ConsoleColor.Red);
                        this._manager_buffer.Clear();
                        session.Close(true);
                        return;
                    }

                    if (packageLen > _manager_buffer.Count - 4)
                        return;

                    byte[] completedBytes = _manager_buffer.GetRange(4, packageLen).ToArray();

                    this.OnMessage(completedBytes);

                    _manager_buffer.RemoveRange(0, packageLen + 4);

                } while (_manager_buffer.Count > 4);
            }
            else
            {
                var sessionBased = session.AppTokens[1] as TcpProxySessionBased;

                sessionBased._receiveTransferredBytes = data.Length;

                this._onSessionNotifyProc(SessionCompletedNotify.OnRecv, sessionBased as SessionHandler);

                sessionBased.Buffer.AddRange(data);
                do
                {
                    if (sessionBased.Buffer.Count < 4)
                        return;

                    byte[] lenBytes = sessionBased.Buffer.GetRange(0, 4).ToArray();
                    int packageLen = BitConverter.ToInt32(lenBytes, 0);

                    //缓冲区越界判断
                    if (packageLen > (1024 * 1024 * 2) || packageLen < 1)
                    {
                        this.ConsoleWriteLine("DEBUG BUFFER OutOfRange !! PacketProcess WORK", ConsoleColor.Red);
                        session.Close(true);
                        return;
                    }

                    if (packageLen > sessionBased.Buffer.Count - 4)
                        return;

                    byte[] completeBytes = sessionBased.Buffer.GetRange(4, packageLen).ToArray();

                    sessionBased._completedBuffer = CompressHelper.Decompress(completeBytes);

                    this._onSessionNotifyProc(SessionCompletedNotify.OnReceived, sessionBased as SessionHandler);

                    sessionBased.Buffer.RemoveRange(0, packageLen + 4);

                } while (sessionBased.Buffer.Count > 4);
            }
        }

        private void OnClosed(TcpSocketSaeaSession session)
        {
            if (session.AppTokens == null && this._manager_login == 0)
            {
                session.AppTokens = new object[]
                {
                    SessionWorkType.ManagerSession,
                    null
                };
            }
            else if (this._manager_login == 1 && session.AppTokens == null)
            {
                return;
            }

            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                Interlocked.Exchange(ref this._manager_login, 0);

                //_manager_buffer.Clear();
                _manager_session = null;

                foreach (var _session in this._sessionList)
                {
                    this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, _session as SessionHandler);
                    _session.Dispose();
                }
                this._sessionList.Clear();

                if (!this._LogOut)
                {
                    this.ConsoleWriteLine("DEBUG ACCEPT CONNECT", ConsoleColor.Green);
                    var timer = new System.Timers.Timer();
                    timer.Interval = 5000;
                    timer.Elapsed += (s, e) =>
                    {
                        this.ConsoleWriteLine("DEBUG RE CONNECT", ConsoleColor.Green);
                        this._clientAgent.ConnectToServer(this._options.ServiceIPEndPoint);

                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }


            }
            else
            {
                var sessionBased = session.AppTokens[1] as TcpProxySessionBased;
                this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, sessionBased as SessionHandler);
                sessionBased.Dispose();
            }
        }
        private void OnMessage(byte[] data)
        {
            MsgCommand cmd = (MsgCommand)data[0];
            switch (cmd)
            {
                case MsgCommand.Msg_Set_Session:
                    this.CreateSession(data);
                    break;
                case MsgCommand.Msg_Connect_Work:
                    this._clientAgent.ConnectToServer(this._options.ServiceIPEndPoint);
                    break;
                case MsgCommand.Msg_LogOut:
                    this.LogOut();
                    break;
                case MsgCommand.Msg_MessageData:
                    this.ProcessPackage(data);
                    break;
                case MsgCommand.Msg_Close_Session:
                    this.ProcessSessionClose(data);
                    break;
                case MsgCommand.Msg_AccessKeyWrong:
                    this.ProcessAccessKeyWrong();
                    break;
            }


        }
        private void ProcessAccessKeyWrong()
        {
            this._LogOut = true;
            if (this._manager_login == 1)
                this._manager_session.Close(true);

            this._onProxyNotify?.Invoke(ProxyNotify.AccessKeyWrong);
        }
        private void ProcessPackage(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 1);
            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));
            TcpProxySessionBased session = gc.Target as TcpProxySessionBased;

            if (session == null)
            {
                this.ConsoleWriteLine("DEBUG ProcessPackage SESSION NULL", ConsoleColor.Red);
                return;
            }

            byte[] bytes = new byte[data.Length - (sizeof(Int64) + 1)];
            Array.Copy(data, sizeof(Int64) + 1, bytes, 0, bytes.Length);

            session._receiveTransferredBytes = bytes.Length;
            this._onSessionNotifyProc(SessionCompletedNotify.OnRecv, session as SessionHandler);
            session.Buffer.AddRange(bytes);
            do
            {
                if (session.Buffer.Count < 4)
                    return;

                byte[] lenBytes = session.Buffer.GetRange(0, 4).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                //缓冲区越界判断
                if (packageLen > (1024 * 1024 * 2) || packageLen < 1)
                {
                    this.ConsoleWriteLine("DEBUG BUFFER OutOfRange !! ProcessPackage DATA", ConsoleColor.Red);
                    this._manager_session.Close(true);
                    return;
                }

                if (packageLen > session.Buffer.Count - 4)
                    return;

                byte[] completeBytes = session.Buffer.GetRange(4, packageLen).ToArray();

                session._completedBuffer = CompressHelper.Decompress(completeBytes);

                this._onSessionNotifyProc(SessionCompletedNotify.OnReceived, session as SessionHandler);

                session.Buffer.RemoveRange(0, packageLen + 4);

            } while (session.Buffer.Count > 4);
        }


        private void LogOut()
        {
            this._LogOut = true;
            if (this._manager_login == 1)
                this._manager_session.Close(true);

            this._onProxyNotify?.Invoke(ProxyNotify.LogOut);
        }

        private void ProcessSessionClose(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 1);
            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));

            TcpProxySessionBased session = gc.Target as TcpProxySessionBased;
            this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, session as SessionHandler);
            this._sessionList.Remove(session);//移除List引用
            session.Buffer.Clear();
            session.Dispose();
            //////////////////////////////check
        }
        private void CreateSession(byte[] data)
        {
            byte[] body = new byte[data.Length - 1];
            Array.Copy(data, 1, body, 0, body.Length);

            //byte[] sessionIds = new byte[body.Length * 2];
            List<byte> buffer = new List<byte>();

            byte[] sessionId = new byte[sizeof(Int64) * 2];

            List<TcpProxySessionBased> sessionList = new List<TcpProxySessionBased>();
            for (int i = 0; i < body.Length / sizeof(Int64); i++)
            {
                Array.Copy(body, i * sizeof(Int64), sessionId, 0, sizeof(Int64));
                TcpProxySessionBased sessionBased = new TcpProxySessionBased(_manager_session);
                sessionBased.RemoteId = BitConverter.ToInt64(sessionId, 0);

                Console.WriteLine("CreateSession:" + sessionBased.RemoteId + " Id:" + sessionBased.Id);

                sessionList.Add(sessionBased);

                BitConverter.GetBytes(sessionBased.Id).CopyTo(sessionId, sizeof(Int64));
                buffer.AddRange(sessionId);
                //Array.Copy(sessionId, 0, sessionIds, i * (sizeof(Int64) + sizeof(Int64)), sizeof(Int64) + sizeof(Int64));
            }

            SendMessageHelper.SendMessage(_manager_session, MsgCommand.Msg_Set_Session_Id, buffer.ToArray());

            this._sessionList.AddRange(sessionList.ToArray());

            foreach (var session in sessionList)
            {
                this._onSessionNotifyProc?.Invoke(SessionCompletedNotify.OnConnected, session as SessionHandler);
            }

            sessionList.Clear();
        }

        private void SendAck(TcpSocketSaeaSession session, SessionWorkType type)
        {
            //                          //命令头      //accesskey     //workType
            byte[] bytes = new byte[sizeof(Int16) + sizeof(Int64) + sizeof(Byte)];
            BitConverter.GetBytes(AckPacket).CopyTo(bytes, 0);
            BitConverter.GetBytes(_options.AccessKey).CopyTo(bytes, 2);
            bytes[10] = (Byte)type;

            byte[] body = CompressHelper.Compress(bytes, 0, bytes.Length);

            byte[] data = new byte[sizeof(Int32) + body.Length];
            BitConverter.GetBytes(body.Length).CopyTo(data, 0);
            body.CopyTo(data, 4);

            session.SendAsync(data);
        }

        private void ConsoleWriteLine(string log, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ResetColor();
        }

        public override void StartSerivce()
        {
            this._LogOut = false;
            this._clientAgent.ConnectToServer(this._options.ServiceIPEndPoint);
        }
        public override void BroadcastAsync(byte[] data)
        {
            foreach (var session in this._sessionList)
                session.SendAsync(data);
        }

        public override void BroadcastAsync(byte[] data, int offset, int lenght)
        {
            foreach (var session in this._sessionList)
                session.SendAsync(data, offset, lenght);
        }

        public override void CloseService()
        {
            this._LogOut = true;
            if (this._manager_login == 1)
                _manager_session.Close(true);
        }

        public override void DisconnectAll()
        {
            foreach (var session in this._sessionList)
                session.SessionClose();
        }
    }
}
