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
    public class TcpSocketSaeaServer : TcpSocketSaeaEngineBased
    {
        private Socket _listener;
        private bool _isRuning = true;
        private TcpSocketSaeaServerConfiguration _config;
        internal TcpSocketSaeaServer(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketSaeaServerConfiguration configuration,
            NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession> completetionNotify)
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

            while (this._isRuning)
            {

                for (int i = 0; i < TcpSocketSaeaSessions.Count; i++)
                {
                    var session = (TcpSocketSaeaPackBased)TcpSocketSaeaSessions[i];
                    if (session.State != TcpSocketConnectionState.Connected)
                    {
                        LogHelper.WriteLog("server_heart_thread != Connected");
                        TcpSocketSaeaSessions.RemoveAt(i); i--;
                        continue;
                    }

                    if ((int)(DateTime.Now - session._heartTime).TotalSeconds > 20)
                    {
                        LogHelper.WriteLog("server_heart_thread_timeout --state：" + session.State.ToString() + " present_time：" + DateTime.Now.ToString() + " heart_time：" + session._heartTime.ToString());
                        if (session.State == TcpSocketConnectionState.Connected)
                        {
                            Console.WriteLine("server timeout--");
                            session.Close(true);
                            TcpSocketSaeaSessions.RemoveAt(i); i--;
                        }
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void Listen(IPEndPoint ipendPoint)
        {
            this._isRuning = true;

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetSocketOptions();
            _listener.Bind(ipendPoint);
            _listener.Listen(_config.PendingConnectionBacklog);

            var awaiter = HandlerSaeaPool.Take();
            SaeaExHelper.AcceptAsync(_listener, awaiter, Accept);
        }

        private void Accept(SaeaAwaiter awaiter, SocketError socketError)
        {
            if (socketError == SocketError.Success)
            {
                var acceptedSocket = awaiter.Saea.AcceptSocket;
                var session = SessionPool.Take();
                session.Attach(acceptedSocket);
                this.TcpSocketSaeaSessions.Add(session);
                this.CompletetionNotify?.Invoke(TcpSessionNotify.OnConnected, session);

                session.StartProcess();
            }
            else
                LogHelper.WriteLog("server_accept-fail");

            if (!this._isRuning)
                return;

            awaiter.Saea.AcceptSocket = null;
            SaeaExHelper.AcceptAsync(_listener, awaiter, Accept);
        }
        private void SetSocketOptions()
        {
            _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Configuration.ReuseAddress);
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
                LogHelper.WriteLog("server_shutdown exception info：" + e.Message);
            }
            finally
            {
                this._isRuning = false;
                base.Dispose();
            }
        }
    }
}
