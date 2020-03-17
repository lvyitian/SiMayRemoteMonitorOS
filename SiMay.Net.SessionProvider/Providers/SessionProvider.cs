using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public abstract class SessionProvider
    {
        public event Action<SessionProviderContext, TcpSessionNotify> SessionNotifyEventHandler;

        protected void SessionNotify(SessionProviderContext providerContext, TcpSessionNotify type)
        {
            SessionNotifyEventHandler?.Invoke(providerContext, type);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public abstract void StartSerivce();

        /// <summary>
        /// 广播发送
        /// </summary>
        /// <param name="data"></param>
        public virtual void BroadcastAsync(byte[] data)
            => BroadcastAsync(data, 0, data.Length);

        /// <summary>
        /// 广播发送
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="lenght"></param>
        public abstract void BroadcastAsync(byte[] data, int offset, int lenght);

        /// <summary>
        /// 关闭所有
        /// </summary>
        public abstract void DisconnectAll();

        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void CloseService();
    }
}