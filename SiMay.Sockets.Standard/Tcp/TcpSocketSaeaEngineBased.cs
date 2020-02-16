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
    public abstract class TcpSocketSaeaEngineBased : IDisposable
    {
        private static readonly byte[] EmptyArray = new byte[0];

        protected List<TcpSocketSaeaSession> TcpSocketSaeaSessions { get; set; }

        protected TcpSocketConfigurationBase Configuration { get; set; }

        protected SaeaAwaiterPool HandlerSaeaPool { get; set; }

        protected SessionPool SessionPool { get; set; }

        protected TcpSocketSaeaSessionType SaeaSessionType { get; set; }

        protected NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession> CompletetionNotify { get; set; }

        public int SessionCount
        {
            get { return TcpSocketSaeaSessions.Count; }
        }


        internal TcpSocketSaeaEngineBased(
            TcpSocketSaeaSessionType saeaSessionType,
            TcpSocketConfigurationBase configuration,
            NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession> completetionNotify)
        {
            TcpSocketSaeaSessions = new List<TcpSocketSaeaSession>();
            HandlerSaeaPool = new SaeaAwaiterPool();
            SessionPool = new SessionPool();

            SaeaSessionType = saeaSessionType;
            Configuration = configuration;
            CompletetionNotify = completetionNotify;

            HandlerSaeaPool.Initialize(() => new SaeaAwaiter(),
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
                        Console.WriteLine("server_clean awaiter info：" + ex.Message);
                    }
                },
                50);
            if (saeaSessionType == TcpSocketSaeaSessionType.Full)
            {
                SessionPool.Initialize(() => new TcpSocketSaeaFullBased(Configuration, HandlerSaeaPool, SessionPool, completetionNotify, this),
                    (session) =>
                    {
                        session.Detach();
                    }, 50);
            }
            else
            {
                SessionPool.Initialize(() => new TcpSocketSaeaPackBased(Configuration, HandlerSaeaPool, SessionPool, completetionNotify, this),
                    (session) =>
                    {
                        session.Detach();
                    }, 50);
            }
        }
        public virtual void BroadcastAsync(byte[] data)
            => BroadcastAsync(data, 0, data.Length);

        public virtual void BroadcastAsync(byte[] data, int offset, int length)
        {
            foreach (TcpSocketSaeaSession session in TcpSocketSaeaSessions)
                session.SendAsync(data, offset, length);
        }
        public virtual void DisconnectAll(bool notify)
        {
            foreach (TcpSocketSaeaSession session in TcpSocketSaeaSessions)
                session.Close(notify);

            TcpSocketSaeaSessions.Clear();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.HandlerSaeaPool.Dispose();
                    this.SessionPool.Dispose();
                    this.DisconnectAll(true);
                    LogHelper.DisposeLogThread();
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TcpSocketSaeaEngineBased()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public virtual void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
