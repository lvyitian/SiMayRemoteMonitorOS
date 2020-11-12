using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.ModelBinder
{
    public class PacketModelBinder<TSession, TMessageHead>
    {
        private bool _init = false;

        private readonly object _lock = new object();

        private ConcurrentDictionary<string, Action<TSession>> _reflectionCache = new ConcurrentDictionary<string, Action<TSession>>();

        private ConcurrentDictionary<string, Func<TSession, object>> _reflectionFuncCache = new ConcurrentDictionary<string, Func<TSession, object>>();

        public bool Contains(TMessageHead head, object source)
        {
            if (!_init)
            {
                lock (_lock)
                {
                    if (!_init)
                    {
                        this.InitCall(source);
                        this._init = true;
                    }
                }
            }

            var sourceName = source.GetType().Name;
            var actionKey = sourceName + "_" + Convert.ToInt16(head);

            if (_reflectionCache.ContainsKey(actionKey))
                return true;

            if (_reflectionFuncCache.ContainsKey(actionKey))
                return true;

            return false;
        }
        private void InitCall(object source)
        {
            var methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault();
                if (attr == null)
                    continue;

                var handlerHead = (attr as PacketHandler).MessageHead;
                var key = source.GetType().Name + "_" + Convert.ToInt16(handlerHead);

                if (method.ReturnType == typeof(void))
                {
                    var targetAction = Delegate.CreateDelegate(typeof(Action<TSession>), source, method) as Action<TSession>;
                    _reflectionCache.TryAdd(key, targetAction);
                }
                else
                {
                    var targetAction = Delegate.CreateDelegate(typeof(Func<TSession, object>), source, method) as Func<TSession, object>;
                    _reflectionFuncCache.TryAdd(key, targetAction);
                }
            }
        }

        public bool CallFunctionPacketHandler(TSession session, TMessageHead head, object source)
            => this.CallFunctionPacketHandler(session, head, source, out _);

        public bool CallFunctionPacketHandler(TSession session, TMessageHead head, object source, out object returnEntity)
        {
            var sourceName = source.GetType().Name;
            var actionKey = sourceName + "_" + Convert.ToInt16(head);

            if (!_init)
            {
                lock (_lock)
                {
                    if (!_init)
                    {
                        this.InitCall(source);
                        this._init = true;
                    }
                }
            }

            returnEntity = null;

            try
            {
                if (_reflectionFuncCache.ContainsKey(actionKey))
                {
                    Func<TSession, object> action;
                    if (_reflectionFuncCache.ContainsKey(actionKey)
                        && _reflectionFuncCache.TryGetValue(actionKey, out action))
                    {
                        returnEntity = action?.Invoke(session);
                        return true;
                    }
                }

                if (_reflectionCache.ContainsKey(actionKey))
                {
                    Action<TSession> action;
                    if (_reflectionCache.ContainsKey(actionKey)
                        && _reflectionCache.TryGetValue(actionKey, out action))
                    {
                        action?.Invoke(session);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            _reflectionFuncCache.Clear();
            _reflectionCache.Clear();
        }
    }
}
