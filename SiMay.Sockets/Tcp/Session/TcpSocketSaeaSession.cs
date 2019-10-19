using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Pooling;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Sockets.Tcp.Session
{
    public abstract class TcpSocketSaeaSession
    {
        protected readonly NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> _notifyEventHandler;
        protected readonly TcpSocketConfigurationBase _configuration;
        protected readonly SaeaAwaiterPool _handlerSaeaPool;
        protected readonly SessionPool _sessionPool;
        protected readonly LogHelper _logger;

        protected readonly TcpSocketSaeaEngineBased _agent;
        protected readonly object _opsLock = new object();
        protected TcpSocketConnectionState _state;
        protected byte[] _completebuffer;
        protected Socket _socket;
        protected DateTime _startTime;

        public int SendBytesTransferred { get; protected set; }
        public int ReceiveBytesTransferred { get; protected set; }
        public byte[] CompletedBuffer { get { return _completebuffer; } }
        public object[] AppTokens { get; set; }
        public Socket Socket { get { return _socket; } }
        public bool Connected { get { return _socket != null && _socket.Connected; } }
        public TcpSocketConnectionState State { get { return _state; } internal set { _state = value; } }
        public DateTime StartTime { get { return _startTime; } }



        internal TcpSocketSaeaSession(
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> notifyEventHandler,
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            TcpSocketSaeaEngineBased agent,
            LogHelper logHelper)
        {
            _notifyEventHandler = notifyEventHandler;
            _configuration = configuration;
            _handlerSaeaPool = handlerSaeaPool;
            _sessionPool = sessionPool;
            _agent = agent;
            _logger = logHelper;
        }

        internal abstract void Attach(Socket socket);

        internal abstract void Detach();

        internal abstract void StartProcess();

        public abstract int Send(byte[] data);
        public abstract int Send(byte[] data, int offset, int length);

        public abstract void SendAsync(byte[] data);

        public abstract void SendAsync(byte[] data, int offset, int lenght);

        public abstract void Close(bool notify);
    }
}
