using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public enum ConnectionWorkType
    {
        /// <summary>
        /// 主被控服务连接
        /// </summary>
        MainServiceConnection,

        /// <summary>
        /// 服务工作连接
        /// </summary>
        ApplicationServiceConnection,

        /// <summary>
        /// 主控端连接
        /// </summary>
        MainApplicationConnection,

        /// <summary>
        /// 应用工作连接
        /// </summary>
        ApplicationConnection,

        /// <summary>
        /// 未识别
        /// </summary>
        None
    }
}
