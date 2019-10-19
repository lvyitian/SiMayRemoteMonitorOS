using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Awaitable;
using SiMay.Sockets.Tcp.Pooling;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Sockets.Tcp
{
    public abstract class TcpSocketSaeaEngineBased
    {
        private static readonly byte[] EmptyArray = new byte[0];
        protected readonly List<TcpSocketSaeaSession> _tcpSocketSaeaSessions = new List<TcpSocketSaeaSession>();
        protected readonly TcpSocketConfigurationBase _configuration;
        protected readonly SaeaAwaiterPool _handlerSaeaPool = new SaeaAwaiterPool();
        protected readonly SessionPool _sessionPool = new SessionPool();
        protected readonly LogHelper _logger = new LogHelper();
        protected NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> _completetionNotify;
        protected TcpSocketSaeaSessionType _SaeaSessionType;

        public int SessionCount
        {
            get { return _tcpSocketSaeaSessions.Count; }
        }

        internal TcpSocketSaeaEngineBased(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketConfigurationBase configuration,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> completetionNotify)
        {
            _SaeaSessionType = saeaSessionType;
            _configuration = configuration;
            _completetionNotify = completetionNotify;

            _handlerSaeaPool.Initialize(() => new SaeaAwaiter(),
                (saea) =>
                {
                    try
                    {
                        saea.Saea.AcceptSocket = null;
                        saea.Saea.SetBuffer(EmptyArray, 0, EmptyArray.Length);
                        saea.Saea.RemoteEndPoint = null;
                        saea.Saea.SocketFlags = SocketFlags.None;
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteLog("server_clean awaiter info：" + ex.Message);
                        Console.WriteLine(ex.Message);
                    }
                },
                50);
            if (saeaSessionType == TcpSocketSaeaSessionType.Full)
            {
                _sessionPool.Initialize(() => new TcpSocketSaeaFullBased(_configuration, _handlerSaeaPool, _sessionPool, completetionNotify, _logger, this),
                    (session) =>
                    {
                        session.Detach();
                    }, 50);
            }
            else
            {
                _sessionPool.Initialize(() => new TcpSocketSaeaPackBased(_configuration, _handlerSaeaPool, _sessionPool, completetionNotify, _logger, this),
                    (session) =>
                    {
                        session.Detach();
                    }, 50);
            }
        }
        public abstract void BroadcastAsync(byte[] data);
        public abstract void BroadcastAsync(byte[] data, int offset, int length);
        public abstract void DisconnectAll(bool notify);
        public abstract void Dispose();
    }
}
