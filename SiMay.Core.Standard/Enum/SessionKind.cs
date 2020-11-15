using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public enum SessionKind : Byte
    {
        /// <summary>
        /// 主服务会话
        /// </summary>
        MAIN_SERVICE_SESSION,

        /// <summary>
        /// 应用服务会话
        /// </summary>
        APP_SERVICE_SESSION,

        /// <summary>
        /// 未知会话
        /// </summary>
        NONE_SESSION,
    }
}
