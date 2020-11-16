using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SiMay.RemoteService.Loader
{
    public interface IAppMainService
    {
        /// <summary>
        /// 主服务启动参数
        /// </summary>
        public StartParameter StartParameter { get; set; }

        /// <summary>
        /// 会话提供对象
        /// </summary>
        public TcpClientSessionProvider SessionProvider { get; set; }

        /// <summary>
        /// 服务器地址
        /// </summary>
        public IPEndPoint RemoteIPEndPoint { get; set; }

        /// <summary>
        /// 初始化启动主服务
        /// </summary>
        /// <param name="session"></param>
        public void StartService(SessionProviderContext session);

        /// <summary>
        /// 消息通知
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="session"></param>
        public void Notify(TcpSessionNotify notify, SessionProviderContext session);
    }
}
