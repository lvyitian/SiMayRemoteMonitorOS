using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService.Loader.Interface
{
    public interface IAppMainService
    {
        void Notify(TcpSessionNotify notify, TcpSocketSaeaSession session);
    }
}
