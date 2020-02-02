using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider
{
    public class SessionProviderOptions
    {
        /// <summary>
        /// Session提供类型
        /// </summary>
        public SessionProviderType SessionProviderType { get; set; }

        /// <summary>
        /// 中间服务识别的唯一ID
        /// </summary>
        public long AccessId { get; set; }

        /// <summary>
        /// 中间服务访问Key
        /// </summary>
        public long AccessKey { get; set; }

        /// <summary>
        /// 中间服务地址
        /// </summary>
        public IPEndPoint ServiceIPEndPoint { get; set; }

        public int PendingConnectionBacklog { get; set; }
    }
}
