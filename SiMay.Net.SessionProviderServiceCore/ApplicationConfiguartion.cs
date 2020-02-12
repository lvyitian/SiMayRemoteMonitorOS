using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class ApplicationConfiguartion
    {
        /// <summary>
        /// 监听地址
        /// </summary>
        public static string LocalAddress { get; set; }

        /// <summary>
        /// 监听端口
        /// </summary>
        public static int ServicePort { get; set; }

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public static long MaxPacketSize { get; set; }

        /// <summary>
        /// 允许主控端匿名登陆
        /// </summary>
        public static bool MainApplicationAnonymous { get; set; }

        /// <summary>
        /// 允许登陆的主控端Id
        /// </summary>
        public static long[] MainApplicationAllowAccessId { get; set; }

        /// <summary>
        /// 连接AccessKey(包含主控端)
        /// </summary>
        public static int AccessKey { get; set; }
    }
}
