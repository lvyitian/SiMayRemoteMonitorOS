using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public enum SessionKind : Byte
    {
        /// <summary>
        /// 主服务连接
        /// </summary>
        MAIN_SERVICE,

        /// <summary>
        /// 应用服务工作连接
        /// </summary>
        APP_SERVICE,

        /// <summary>
        /// 未识别的连接
        /// </summary>
        NONE,
    }
}
