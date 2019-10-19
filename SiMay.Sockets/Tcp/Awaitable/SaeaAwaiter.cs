using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SiMay.Sockets.Tcp.Awaitable
{
    public sealed class SaeaAwaiter : IDisposable
    {
        private readonly SocketAsyncEventArgs _saea = new SocketAsyncEventArgs();
        private static readonly Action<SaeaAwaiter, SocketError> SENTINEL = delegate { };
        private Action<SaeaAwaiter, SocketError> _continuation;

        public SocketAsyncEventArgs Saea
        {
            get { return _saea; }
        }

        internal SaeaAwaiter()
        {
            _saea.Completed += OnSaeaCompleted;
        }
        private void OnSaeaCompleted(object sender, SocketAsyncEventArgs args)
        {
            var continuation = _continuation ?? Interlocked.CompareExchange(ref _continuation, SENTINEL, null);

            if (continuation != null)
            {
                continuation.Invoke(this, _saea.SocketError);
            }
        }

        internal void OnCompleted(Action<SaeaAwaiter, SocketError> continuation)
        {
            if (_continuation == SENTINEL
                || Interlocked.CompareExchange(ref _continuation, continuation, null) == SENTINEL)
            {
                continuation.Invoke(this, _saea.SocketError);
            }
        }

        internal void Reset()
        {
            _continuation = null;
        }

        public void Dispose()
        {
            _saea.Dispose();
            _continuation = null;
        }
    }
}
