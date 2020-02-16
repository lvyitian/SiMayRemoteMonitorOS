using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Awaitable;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;

namespace SiMay.Sockets.Tcp.Client
{
    public class TcpSocketSaeaClientAgent : TcpSocketSaeaEngineBased
    {
        private bool _isRuning = true;

        internal TcpSocketSaeaClientAgent(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaClientConfiguration configuration,
            NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession> completetionNotify)
            : base(saeaSessionType, configuration, completetionNotify)
        {

            if (configuration.AppKeepAlive &&
                saeaSessionType == TcpSocketSaeaSessionType.Packet)
            {
                var keepAliveThread = new Thread(AppKeepAliveWorkThread);
                keepAliveThread.IsBackground = true;
                keepAliveThread.Start();
            }
        }

        private void AppKeepAliveWorkThread()
        {
            byte[] emptyHeart = new byte[] { 0, 0, 0, 0 };

            while (this._isRuning)
            {
                for (int i = 0; i < TcpSocketSaeaSessions.Count; i++)
                {
                    var session = (TcpSocketSaeaPackBased)TcpSocketSaeaSessions[i];
                    if (session.State == TcpSocketConnectionState.Closed)
                    {
                        LogHelper.WriteLog("client_heart_thread = Closed");
                        TcpSocketSaeaSessions.RemoveAt(i); i--;
                        continue;
                    }

                    if ((int)(DateTime.Now - session._heartTime).TotalSeconds > 20)
                    {
                        LogHelper.WriteLog("client_heart_thread_timeout ! state：" + session.State.ToString() + " present_time：" + DateTime.Now.ToString() + " heart_time：" + session._heartTime.ToString());
                        if (session.State == TcpSocketConnectionState.Connected)
                        {
                            session.Close(true);
                            TcpSocketSaeaSessions.RemoveAt(i); i--;
                        }
                    }
                    else
                    {
                        //如果数据正在发送，就不发送心跳
                        if (session._intervalIsUseChannel == 0)
                        {
                            var awaiter = HandlerSaeaPool.Take();

                            //4个字节空包头
                            awaiter.Saea.SetBuffer(emptyHeart, 0, emptyHeart.Length);
                            SaeaExHelper.SendAsync(session.Socket, awaiter, (a, e) => HandlerSaeaPool.Return(a));
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void ConnectToServer(IPEndPoint ipEndPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var awaiter = HandlerSaeaPool.Take();
            awaiter.Saea.RemoteEndPoint = ipEndPoint;

            SaeaExHelper.ConnectAsync(socket, awaiter, (a, e) =>
             {
                 HandlerSaeaPool.Return(a);
                 var session = SessionPool.Take();
                 if (e != SocketError.Success)
                 {
                     LogHelper.WriteLog("client_connect-error");
                     CompletetionNotify?.Invoke(TcpSessionNotify.OnClosed, session);
                     SessionPool.Return(session);
                     return;
                 }
                 TcpSocketSaeaSessions.Add(session);

                 session.Attach(socket);
                 CompletetionNotify?.Invoke(TcpSessionNotify.OnConnected, session);
                 session.StartProcess();
             });
        }

        public override void Dispose()
        {
            this._isRuning = false;
            base.Dispose();
        }
    }
}
