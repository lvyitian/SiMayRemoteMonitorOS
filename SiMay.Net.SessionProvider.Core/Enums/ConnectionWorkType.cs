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
        [Description("主服务连接")]
        MainServiceConnection,

        /// <summary>
        /// 服务工作连接
        /// </summary>
        [Description("应用服务连接")]
        ApplicationServiceConnection,

        /// <summary>
        /// 主控端连接
        /// </summary>
        [Description("主控端连接")]
        MainApplicationConnection,

        /// <summary>
        /// 应用工作连接
        /// </summary>
        [Description("应用连接")]
        ApplicationConnection,

        /// <summary>
        /// 未识别
        /// </summary>
        [Description("未知连接")]
        None
    }
}
