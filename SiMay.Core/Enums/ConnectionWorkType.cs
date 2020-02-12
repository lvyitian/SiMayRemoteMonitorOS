using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Enums
{
    public enum ConnectionWorkType : Byte
    {
        /// <summary>
        /// 主服务连接
        /// </summary>
        MAINCON,

        /// <summary>
        /// 应用服务工作连接
        /// </summary>
        WORKCON,

        /// <summary>
        /// 未识别的连接
        /// </summary>
        NONE,
    }
}
