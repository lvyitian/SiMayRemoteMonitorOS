using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Enums
{
    public enum ConnectionWorkType : Byte
    {
        MAINCON,//主连接
        WORKCON,//工作连接
        NONE,//未确认的连接
    }
}
