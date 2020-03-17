using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SiMay.Sockets.Tcp.Awaitable;

namespace SiMay.Sockets.Tcp.Pooling
{
    public class SaeaAwaiterPool : ObjectPool<SaeaAwaiter>
    {
        private Func<SaeaAwaiter> _createSaea;
        private Action<SaeaAwaiter> _cleanSaea;

        public SaeaAwaiterPool Initialize(Func<SaeaAwaiter> createSaea, Action<SaeaAwaiter> cleanSaea, int initialCount = 0)
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

        protected override SaeaAwaiter Create()
        {
            return _createSaea();
        }

        public void Return(SaeaAwaiter saea)
        {
            _cleanSaea(saea);
            Add(saea);
        }
    }
}
