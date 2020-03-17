using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    internal class ApplicationConfiguartion
    {
        public static void SetOptions(StartServiceOptions options) => Options = options;
        public static StartServiceOptions Options { get; private set; }
    }

    public class StartServiceOptions
    {
        /// <summary>
        /// 监听地址
        /// </summary>
        public string LocalAddress { get; set; }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 最大数据包长度
        /// </summary>
        public long MaxPacketSize { get; set; }

        /// <summary>
        /// 允许主控端匿名登陆
        /// </summary>
        public bool MainApplicationAnonyMous { get; set; }

        /// <summary>
        /// 允许登陆的主控端Id
        /// </summary>
        public long[] MainApplicationAllowAccessId { get; set; }

        /// <summary>
        /// 主控端登陆Key
        /// </summary>
        public long MainAppAccessKey { get; set; }

        /// <summary>
        /// 连接AccessKey
        /// </summary>
        public long AccessKey { get; set; }
    }
}
