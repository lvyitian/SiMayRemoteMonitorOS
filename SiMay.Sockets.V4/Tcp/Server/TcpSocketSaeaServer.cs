using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Awaitable;
using SiMay.Sockets.Tcp.Pooling;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;

namespace SiMay.Sockets.Tcp.Server
{
    public class TcpSocketSaeaServer : TcpSocketSaeaEngineBased, IDisposable
    {
        Socket _listener;
        bool _isruning = true;
        TcpSocketSaeaServerConfiguration _config;
        internal TcpSocketSaeaServer(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaServerConfiguration configuration,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> completetionNotify)
            : base(saeaSessionType, configuration, completetionNotify)
        {
            this._config = configuration;
            if (configuration.AppKeepAlive &&
                saeaSessionType == TcpSocketSaeaSessionType.Packet)
            {
                var keepAliveThread = new Thread(AppKeepAliveWorkThread);
                keepAliveThread.IsBackground = true;
                keepAliveThread.Start();
            }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return (IPEndPoint)_listener.LocalEndPoint; }
        }

        private void AppKeepAliveWorkThread()
        {
            byte[] emptyHeart = new byte[] { 0, 0, 0, 0 };

            while (this._isruning)
            {

                for (int i = 0; i < _tcpSocketSaeaSessions.Count; i++)
                {
                    var session = (TcpSocketSaeaPackBased)_tcpSocketSaeaSessions[i];
                    if (session.State != TcpSocketConnectionState.Connected)
                    {
                        _logger.WriteLog("server_heart_thread != Connected");
                        _tcpSocketSaeaSessions.RemoveAt(i); i--;
                        continue;
                    }

                    //if (session.State == TcpSocketConnectionState.None
                    //|| session.State == TcpSocketConnectionState.Connecting)
                    //    continue;

                    if ((int)(DateTime.Now - session._heartTime).TotalSeconds > 20)
                    {
                        _logger.WriteLog("server_heart_thread_timeout --state：" + session.State.ToString() + " present_time：" + DateTime.Now.ToString() + " heart_time：" + session._heartTime.ToString());
                        if (session.State == TcpSocketConnectionState.Connected)
                        {
                            Console.WriteLine("server timeout--");
                            session.Close(true);
                            _tcpSocketSaeaSessions.RemoveAt(i); i--;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void Listen(IPEndPoint ipendPoint)
        {
            this._isruning = true;

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetSocketOptions();

            _listener.Bind(ipendPoint);

            _listener.Listen(_config.PendingConnectionBacklog);

            var awaiter = _handlerSaeaPool.Take();
            SaeaExHelper.AcceptAsync(_listener, awaiter, Accept);
        }

        private void Accept(SaeaAwaiter awaiter, SocketError socketError)
        {
            if (socketError == SocketError.Success)
            {
                var acceptedSocket = awaiter.Saea.AcceptSocket;
                var session = _sessionPool.Take();
                session.Attach(acceptedSocket);
                this._tcpSocketSaeaSessions.Add(session);
                this._completetionNotify?.Invoke(TcpSocketCompletionNotify.OnConnected, session);

                session.StartProcess();
            }
            else _logger.WriteLog("server_accept-fail");

            if (!this._isruning) return;

            awaiter.Saea.AcceptSocket = null;
            SaeaExHelper.AcceptAsync(_listener, awaiter, Accept);
        }
        private void SetSocketOptions()
        {
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, _configuration.ReuseAddress);
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
            try
            {
                _listener.Close(0);
                _listener = null;
            }
            catch (Exception e)
            {
                _logger.WriteLog("server_shutdown exception info：" + e.Message);
            }

            this._logger.Dispose();
            this._isruning = false;
            this.DisconnectAll(false);
            _handlerSaeaPool.Dispose();
            _sessionPool.Dispose();
        }
    }
}
