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

        private bool _initFunctionCache = false;

        private ConcurrentDictionary<string, Action<TSession>> _reflectionCache = new ConcurrentDictionary<string, Action<TSession>>();

        private ConcurrentDictionary<string, Func<TSession, object>> _reflectionFuncCache = new ConcurrentDictionary<string, Func<TSession, object>>();

        public bool CallPacketHandler(TSession session, TMessageHead head, object source)
        {
            var sourceName = source.GetType().Name;
            var actionKey = sourceName + "_" + Convert.ToInt16(head);

            if (!_init)
            {
                Init();
                _init = true;
            }

            Action<TSession> action;
            if (_reflectionCache.ContainsKey(actionKey)
                && _reflectionCache.TryGetValue(actionKey, out action))
            {
                action?.Invoke(session);
                return true;
            }
            else
                return false;

            void Init()
            {
                var methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var method in methods)
                {
                    if (method.ReturnType != typeof(void))
                        continue;

                    var attr = method.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault();
                    if (attr == null)
                        continue;

                    var handlerHead = (attr as PacketHandler).MessageHead;
                    var key = source.GetType().Name + "_" + Convert.ToInt16(handlerHead);
                    var targetAction = Delegate.CreateDelegate(typeof(Action<TSession>), source, method) as Action<TSession>;
                    _reflectionCache.TryAdd(key, targetAction);
                }
            }
        }

        public object CallFunctionPacketHandler(TSession session, TMessageHead head, object source)
        {
            var sourceName = source.GetType().Name;
            var actionKey = sourceName + "_" + Convert.ToInt16(head);

            if (!_initFunctionCache)
            {
                Init();
                _initFunctionCache = true;
            }

            Func<TSession, object> action;
            if (_reflectionFuncCache.ContainsKey(actionKey)
                && _reflectionFuncCache.TryGetValue(actionKey, out action))
            {
                return action?.Invoke(session);
            }
            else
                return null;

            void Init()
            {
                var methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var method in methods)
                {
                    if (method.ReturnType == typeof(void))
                        continue;

                    var attr = method.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault();
                    if (attr == null)
                        continue;

                    var handlerHead = (attr as PacketHandler).MessageHead;
                    var key = source.GetType().Name + "_" + Convert.ToInt16(handlerHead);
                    var targetAction = Delegate.CreateDelegate(typeof(Func<TSession, object>), source, method) as Func<TSession, object>;
                    _reflectionFuncCache.TryAdd(key, targetAction);
                }
            }
        }

        public void Dispose()
        {
            _reflectionFuncCache.Clear();
            _reflectionCache.Clear();
        }
    }
}
