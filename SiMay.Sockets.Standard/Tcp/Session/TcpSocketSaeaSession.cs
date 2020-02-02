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
        protected NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> NotifyEventHandler { get; set; }

        protected TcpSocketConfigurationBase Configuration { get; set; }

        protected SaeaAwaiterPool HandlerSaeaPool { get; set; }

        protected SessionPool SessionPools { get; set; }

        public TcpSocketSaeaEngineBased Agent { get; set; }

        public byte[] CompletedBuffer { get; protected set; }

        public int SendTransferredBytes { get; protected set; }

        public int ReceiveBytesTransferred { get; protected set; }

        public object[] AppTokens { get; set; }

        public Socket Socket { get; protected set; }

        public bool Connected => Socket != null && Socket.Connected;

        public TcpSocketConnectionState State { get; protected set; }

        public DateTime StartTime { get; protected set; }

        internal TcpSocketSaeaSession(
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> notifyEventHandler,
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            TcpSocketSaeaEngineBased agent)
        {
            NotifyEventHandler = notifyEventHandler;
            Configuration = configuration;
            HandlerSaeaPool = handlerSaeaPool;
            SessionPools = sessionPool;
            Agent = agent;
        }

        internal abstract void Attach(Socket socket);

        internal abstract void Detach();

        internal abstract void StartProcess();

        public abstract void SendAsync(byte[] data);

        public abstract void SendAsync(byte[] data, int offset, int lenght);

        public abstract void Close(bool notify);
    }
}
