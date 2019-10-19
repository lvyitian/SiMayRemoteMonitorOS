using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Enums
{
    public enum SystemSessionType : byte
    {
        Shutdown = 0,
        Reboot = 1,
        RegStart = 2,
        RegCancelStart = 3,
        AttributeHide = 4,
        AttributeShow = 5,
        Unstall = 6
    }
}
