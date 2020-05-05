using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public enum ProxyProviderNotify
    {
        [Description("AccessId、Key验证不通过")]
        AccessIdOrKeyWrong,

        [Description("登出")]
        LogOut
    }
}
