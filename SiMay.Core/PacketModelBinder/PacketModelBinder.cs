using SiMay.Core.PacketModelBinder.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SiMay.Core.PacketModelBinding
{
    public class PacketModelBinder<TSession>
    {
        private bool _init = false;
        private ConcurrentDictionary<string, Action<TSession>> _reflectionCache = new ConcurrentDictionary<string, Action<TSession>>();
        public bool InvokePacketHandler(TSession session, MessageHead head, object source)
        {
            var sourceName = source.GetType().Name;
            var actionKey = sourceName + "_" + (short)head;

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
                var methods = source.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault();
                    if (attr == null)
                        continue;

                    var handlerHead = (attr as PacketHandler).MessageHead;
                    var key = source.GetType().Name + "_" + (short)handlerHead;
                    var targetAction = Delegate.CreateDelegate(typeof(Action<TSession>), source, method) as Action<TSession>;
                    _reflectionCache.TryAdd(key, targetAction);
                }
            }
        }

        public void Dispose()
        {
            _reflectionCache.Clear();
        }
    }
}
