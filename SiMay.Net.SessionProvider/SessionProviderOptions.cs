using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider
{
    internal class ApplicationConfiguartion
    {
        public static void SetOptions(SessionProviderOptions options) => Options = options;
        public static SessionProviderOptions Options { get; private set; }
    }

    public class SessionProviderOptions
    {
        /// <summary>
        /// 中间服务识别的唯一ID
        /// </summary>
        public long AccessId { get; set; }

        /// <summary>
        /// 主控端连接Key
        /// </summary>
        public long MainAppAccessKey { get; set; }

        /// <summary>
        /// 中间服务访问Key
        /// </summary>
        public long AccessKey { get; set; }

        /// <summary>
        /// 连接挂起队列
        /// </summary>
        public int PendingConnectionBacklog { get; set; }

        /// <summary>
        /// 最大包长度
        /// </summary>
        public int MaxPacketSize { get; set; }

        /// <summary>
        /// Session提供类型
        /// </summary>
        public SessionProviderType SessionProviderType { get; set; }

        /// <summary>
        /// 中间服务地址
        /// </summary>
        public IPEndPoint ServiceIPEndPoint { get; set; }
    }
}
