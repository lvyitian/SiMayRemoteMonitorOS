using SiMay.Net.SessionProvider;
using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService.Loader
{
    public interface IAppMainService
    {
        void Notify(TcpSessionNotify notify, SessionProviderContext session);
    }
}
