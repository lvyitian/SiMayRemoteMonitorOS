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
using SiMay.Net.SessionProvider.Notify;
using SiMay.Basic;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Core.Packets;
using SiMay.Core.Enums;
using static SiMay.Serialize.Standard.PacketSerializeHelper;

namespace SiMay.Net.SessionProvider.Providers
{
    /// <summary>
    /// 代理协议写的有些冗余。。有空抽出时间重构
    /// </summary>
    public class TcpProxySessionProviderHandle : SessionProvider
    {
        private const Int16 AckHead = 1000;
        private const Int16 CHANNEL_LOGOUT = 0;
        private const Int16 CHANNEL_LOGIN = 1;

        bool _LogOut = false;
        int _managerLoginSign = 0;
        private OnProxyNotify<ProxyNotify> _onProxyNotify;
        private List<byte> _dataBuffer = new List<byte>();
        private List<TcpProxySessionBased> _tcpProxySessionList = new List<TcpProxySessionBased>();
        private TcpSocketSaeaClientAgent _clientAgent;
        private SessionProviderOptions _options;
        private TcpSocketSaeaSession _managerSession;

        /// <summary>
        /// session代理提供器构造函数
        /// </summary>
        /// <param name="options">代理配置设置</param>
        /// <param name="onSessionNotifyProc">session事件通知</param>
        /// <param name="onProxyNotify">代理事件通知</param>
        internal TcpProxySessionProviderHandle(
            SessionProviderOptions options,
            OnSessionNotify<SessionCompletedNotify, SessionProviderContext> onSessionNotifyProc,
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
                        this.ConnectedHandler(session);
                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        this.OnSend(session);
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        this.PacketHandler(session);
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        this.OnClosed(session);
                        break;
                    default:
                        break;
                }
            });
        }

        private void ConnectedHandler(TcpSocketSaeaSession session)
        {
            if (Interlocked.Exchange(ref _managerLoginSign, CHANNEL_LOGIN) == CHANNEL_LOGOUT)
            {
                //表示代理管理连接登陆
                this.SendAck(session, SessionWorkType.ManagerSession);

                _managerSession = session;
                session.AppTokens = new object[]
                {
                    SessionWorkType.ManagerSession,
                    null
                };

                //获取所有主连接
                MessageHelper.SendMessage(session, MessageHead.Msg_Pull_Session);
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

                this._onSessionNotifyProc(SessionCompletedNotify.OnConnected, sessionBased as SessionProviderContext);
            }
        }
        private void OnSend(TcpSocketSaeaSession session)
        {
            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerWorkSession)
            {
                var sessionBased = session.AppTokens[1] as TcpProxySessionBased;
                sessionBased._sendTransferredBytes = session.SendTransferredBytes;
                this._onSessionNotifyProc(SessionCompletedNotify.OnSend, sessionBased as SessionProviderContext);
            }
        }

        private void PacketHandler(TcpSocketSaeaSession session)
        {
            byte[] data = new byte[session.ReceiveBytesTransferred];
            Array.Copy(session.CompletedBuffer, 0, data, 0, data.Length);

            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                _dataBuffer.AddRange(data);
                do
                {
                    if (_dataBuffer.Count < 4)
                        return;

                    byte[] lenBytes = _dataBuffer.GetRange(0, 4).ToArray();
                    int packageLen = BitConverter.ToInt32(lenBytes, 0);

                    //缓冲区越界判断
                    if (packageLen > (1024 * 1024 * 2) || packageLen < 1)
                    {
                        this.ConsoleWriteLine("DEBUG BUFFER OutOfRange !! PacketProcess MAIN", ConsoleColor.Red);
                        this._dataBuffer.Clear();
                        session.Close(true);
                        return;
                    }

                    if (packageLen > _dataBuffer.Count - 4)
                        return;

                    byte[] completedBytes = _dataBuffer.GetRange(4, packageLen).ToArray();

                    this.OnMessage(completedBytes);

                    _dataBuffer.RemoveRange(0, packageLen + 4);

                } while (_dataBuffer.Count > 4);
            }
            else
            {
                var sessionBased = session.AppTokens[1] as TcpProxySessionBased;

                sessionBased._receiveTransferredBytes = data.Length;

                this._onSessionNotifyProc(SessionCompletedNotify.OnRecv, sessionBased as SessionProviderContext);

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

                    sessionBased._completedBuffer = GZipHelper.Decompress(completeBytes);

                    this._onSessionNotifyProc(SessionCompletedNotify.OnReceived, sessionBased as SessionProviderContext);

                    sessionBased.Buffer.RemoveRange(0, packageLen + 4);

                } while (sessionBased.Buffer.Count > 4);
            }
        }

        private void OnClosed(TcpSocketSaeaSession session)
        {
            if (session.AppTokens == null && this._managerLoginSign == 0)
            {
                session.AppTokens = new object[]
                {
                    SessionWorkType.ManagerSession,
                    null
                };
            }
            else if (this._managerLoginSign == 1 && session.AppTokens == null)
            {
                return;
            }

            if ((SessionWorkType)session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                Interlocked.Exchange(ref this._managerLoginSign, 0);

                //_manager_buffer.Clear();
                _managerSession = null;

                foreach (var _session in this._tcpProxySessionList)
                {
                    this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, _session as SessionProviderContext);
                    _session.Dispose();
                }
                this._tcpProxySessionList.Clear();

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
                this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, sessionBased as SessionProviderContext);
                sessionBased.Dispose();
            }
        }
        private void OnMessage(byte[] data)
        {
            MessageHead cmd = (MessageHead)data[0];
            switch (cmd)
            {
                case MessageHead.Msg_Set_Session:
                    this.CreateSession(data);
                    break;
                case MessageHead.Msg_Connect_Work:
                    this._clientAgent.ConnectToServer(this._options.ServiceIPEndPoint);
                    break;
                case MessageHead.Msg_LogOut:
                    this.LogOut();
                    break;
                case MessageHead.Msg_MessageData:
                    this.ProcessPackage(data);
                    break;
                case MessageHead.Msg_Close_Session:
                    this.ProcessSessionClose(data);
                    break;
                case MessageHead.Msg_AccessKeyWrong:
                    this.ProcessAccessKeyWrong();
                    break;
            }


        }
        private void ProcessAccessKeyWrong()
        {
            this._LogOut = true;
            if (this._managerLoginSign == 1)
                this._managerSession.Close(true);

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
            this._onSessionNotifyProc(SessionCompletedNotify.OnRecv, session as SessionProviderContext);
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
                    this._managerSession.Close(true);
                    return;
                }

                if (packageLen > session.Buffer.Count - 4)
                    return;

                byte[] completeBytes = session.Buffer.GetRange(4, packageLen).ToArray();

                session._completedBuffer = GZipHelper.Decompress(completeBytes);

                this._onSessionNotifyProc(SessionCompletedNotify.OnReceived, session as SessionProviderContext);

                session.Buffer.RemoveRange(0, packageLen + 4);

            } while (session.Buffer.Count > 4);
        }


        private void LogOut()
        {
            this._LogOut = true;
            if (this._managerLoginSign == 1)
                this._managerSession.Close(true);

            this._onProxyNotify?.Invoke(ProxyNotify.LogOut);
        }

        private void ProcessSessionClose(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 1);
            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));

            TcpProxySessionBased session = gc.Target as TcpProxySessionBased;
            this._onSessionNotifyProc(SessionCompletedNotify.OnClosed, session as SessionProviderContext);
            this._tcpProxySessionList.Remove(session);//移除List引用
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
                TcpProxySessionBased sessionBased = new TcpProxySessionBased(_managerSession);
                sessionBased.RemoteId = BitConverter.ToInt64(sessionId, 0);

                Console.WriteLine("CreateSession:" + sessionBased.RemoteId + " Id:" + sessionBased.Id);

                sessionList.Add(sessionBased);

                BitConverter.GetBytes(sessionBased.Id).CopyTo(sessionId, sizeof(Int64));
                buffer.AddRange(sessionId);
                //Array.Copy(sessionId, 0, sessionIds, i * (sizeof(Int64) + sizeof(Int64)), sizeof(Int64) + sizeof(Int64));
            }

            MessageHelper.SendMessage(_managerSession, MessageHead.Msg_Set_Session_Id, buffer.ToArray());

            this._tcpProxySessionList.AddRange(sessionList.ToArray());

            foreach (var session in sessionList)
            {
                this._onSessionNotifyProc?.Invoke(SessionCompletedNotify.OnConnected, session as SessionProviderContext);
            }

            sessionList.Clear();
        }

        private void SendAck(TcpSocketSaeaSession session, SessionWorkType type)
        {
            var typeByte = (byte)type;
            var ackBody = SerializePacket(new AckPack()
            {
                Type = typeByte.ConvertTo<ConnectionWorkType>(),
                AccessId = _options.AccessId,
                AccessKey = _options.AccessKey
            });
            ackBody = GZipHelper.Compress(ackBody, 0, ackBody.Length);

            var dataBuilder = new List<byte>();
            dataBuilder.AddRange(BitConverter.GetBytes(ackBody.Length));
            dataBuilder.AddRange(ackBody);
            ackBody = dataBuilder.ToArray();
            dataBuilder.Clear();
            session.SendAsync(ackBody);//构造发送
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
            foreach (var session in this._tcpProxySessionList)
                session.SendAsync(data);
        }

        public override void BroadcastAsync(byte[] data, int offset, int lenght)
        {
            foreach (var session in this._tcpProxySessionList)
                session.SendAsync(data, offset, lenght);
        }

        public override void CloseService()
        {
            this._LogOut = true;
            if (this._managerLoginSign == 1)
                _managerSession.Close(true);
        }

        public override void DisconnectAll()
        {
            foreach (var session in this._tcpProxySessionList)
                session.SessionClose();
        }
    }
}
