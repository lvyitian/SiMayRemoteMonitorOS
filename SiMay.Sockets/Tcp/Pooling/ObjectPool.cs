using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp.Pooling
{
    public abstract class ObjectPool<T> : IDisposable
    {
        private readonly Stack<T> _stack;
        private readonly object _opLock = new object();
        private bool _isDisposed;

        public ObjectPool()
        {
            _stack = new Stack<T>();
        }

        protected abstract T Create();

        public int Count
        {
            get
            {
                return _stack.Count;
            }
        }

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            lock (_opLock)
            {
                _stack.Push(item);
            }
        }

        public T Take()
        {
            T item;
            lock (_opLock)
            {
                if (_stack.Count > 0)
                    item = _stack.Pop();
                else
                    item = Create();
            }
            return item;
        }

        public bool IsDisposed
        {
            get
            {
                lock (_opLock)
                {
                    return _isDisposed;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_opLock)
            {
                if (_isDisposed)
                    return;

                if (disposing)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        var item = this.Take() as IDisposable;
                        if (item != null)
                            item.Dispose();
                    }

                }

                _isDisposed = true;
            }
        }
    }
}
