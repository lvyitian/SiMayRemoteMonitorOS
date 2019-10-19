using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService.Delagate
{
    public delegate void TcpChannelContextNotifyHandler<T1, T2>(T1 arg1, T2 arg2);
    public delegate void TcpChannelContextNotifyHandler<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
}
