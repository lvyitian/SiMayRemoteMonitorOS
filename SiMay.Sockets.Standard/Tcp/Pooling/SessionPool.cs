using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Sockets.Tcp.Session;

namespace SiMay.Sockets.Tcp.Pooling
{
    public class SessionPool : ObjectPool<TcpSocketSaeaSession>
    {
        private Func<TcpSocketSaeaSession> _createSaea;
        private Action<TcpSocketSaeaSession> _cleanSaea;

        public SessionPool Initialize(Func<TcpSocketSaeaSession> createSaea, Action<TcpSocketSaeaSession> cleanSaea, int initialCount = 0)
        {
            if (createSaea == null)
                throw new ArgumentNullException("createSaea");
            if (cleanSaea == null)
                throw new ArgumentNullException("cleanSaea");

            _createSaea = createSaea;
            _cleanSaea = cleanSaea;

            if (initialCount < 0)
                throw new ArgumentOutOfRangeException(
                    "initialCount",
                    initialCount,
                    "Initial count must not be less than zero.");

            for (int i = 0; i < initialCount; i++)
            {
                Add(Create());
            }

            return this;
        }

        protected override TcpSocketSaeaSession Create()
        {
            return _createSaea();
        }

        public void Return(TcpSocketSaeaSession saea)
        {
            _cleanSaea(saea);
            Add(saea);
        }
    }
}
