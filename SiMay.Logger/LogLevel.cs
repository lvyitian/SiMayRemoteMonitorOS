using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiMay.Logger
{
    public enum LogLevel
    {
        [Description("调试")]
        Debug,

        [Description("输出")]
        Information,

        [Description("警告")]
        Warning,

        [Description("异常")]
        Error
    }
}
