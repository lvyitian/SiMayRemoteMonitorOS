using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControls.Core
{
    public class SessionSyncContext
    {
        public SessionSyncContext(SessionProviderContext session, IDictionary<string, object> dictions)
        {
            _session = session;
            _keyDictions = dictions;
        }

        /// <summary>
        /// 获取主服务连接上下文
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                if (_keyDictions.ContainsKey(key))
                    return _keyDictions[key];
                else
                    return null;
            }
            set
            {
                _keyDictions[key] = value;
            }
        }

        public string UniqueId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 主服务连接
        /// </summary>
        private SessionProviderContext _session;
        public SessionProviderContext Session
            => _session;

        private IDictionary<string, object> _keyDictions;

        /// <summary>
        /// 上下文是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
            => _keyDictions.ContainsKey(key);
    }
}
