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

namespace SiMay.Sockets.Tcp.Client
{
    public class TcpSocketSaeaClientAgent : TcpSocketSaeaEngineBased, IDisposable
    {
        private bool _isruning = true;

        internal TcpSocketSaeaClientAgent(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaClientConfiguration configuration,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> completetionNotify)
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

            while (this._isruning)
            {
                for (int i = 0; i < _tcpSocketSaeaSessions.Count; i++)
                {
                    var session = (TcpSocketSaeaPackBased)_tcpSocketSaeaSessions[i];
                    if (session.State == TcpSocketConnectionState.Closed ||
                    session.State == TcpSocketConnectionState.None)
                    {
                        _logger.WriteLog("client_heart_thread = Closed || None");
                        _tcpSocketSaeaSessions.RemoveAt(i); i--;
                        continue;
                    }

                    if (session.State == TcpSocketConnectionState.Connecting)
                        continue;

                    if ((int)(DateTime.Now - session._heartTime).TotalSeconds > 20)
                    {
                        _logger.WriteLog("client_heart_thread_timeout ! state：" + session.State.ToString() + " present_time：" + DateTime.Now.ToString() + " heart_time：" + session._heartTime.ToString());
                        if (session.State == TcpSocketConnectionState.Connected)
                        {
                            session.Close(true);
                            _tcpSocketSaeaSessions.RemoveAt(i); i--;
                        }
                    }
                    else
                    {
                        //如果数据正在发送，就不发送心跳
                        if (session._isuchannel == 0)
                        {
                            var awaiter = _handlerSaeaPool.Take();

                            //4个字节空包头
                            awaiter.Saea.SetBuffer(emptyHeart, 0, emptyHeart.Length);
                            SaeaExHelper.SendAsync(session.Socket, awaiter, (a, e) => _handlerSaeaPool.Return(a));
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void ConnectToServer(IPEndPoint ipEndPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var awaiter = _handlerSaeaPool.Take();
            var session = _sessionPool.Take();

            session.State = TcpSocketConnectionState.Connecting;
            awaiter.Saea.RemoteEndPoint = ipEndPoint;

            SaeaExHelper.ConnectAsync(socket, awaiter, (a, e) =>
             {
                 _handlerSaeaPool.Return(a);
                 if (e != SocketError.Success)
                 {
                     _logger.WriteLog("client_connect-error");
                     _completetionNotify?.Invoke(TcpSocketCompletionNotify.OnClosed, session);
                     _sessionPool.Return(session);
                     return;
                 }
                 _tcpSocketSaeaSessions.Add(session);

                 session.Attach(socket);
                 _completetionNotify?.Invoke(TcpSocketCompletionNotify.OnConnected, session);
                 session.StartProcess();
             });
        }

        public override void BroadcastAsync(byte[] data)
        {
            BroadcastAsync(data, 0, data.Length);
        }
        public override void BroadcastAsync(byte[] data, int offset, int length)
        {
            foreach (TcpSocketSaeaSession session in _tcpSocketSaeaSessions)
                session.SendAsync(data, offset, length);
        }

        public override void DisconnectAll(bool notify)
        {
            foreach (TcpSocketSaeaSession session in _tcpSocketSaeaSessions)
                session.Close(notify);

            _tcpSocketSaeaSessions.Clear();
        }
        public override void Dispose()
        {
            if (this._isruning)
            {
                _isruning = false;
                this._logger.Dispose();
                this.DisconnectAll(false);
                this._handlerSaeaPool.Dispose();
                this._sessionPool.Dispose();
            }
        }
    }
}
