using SiMay.Net.SessionProvider.Delegate;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Net.SessionProvider.SessionBased;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider
{
    public class SessionProviderFactory
    {
        public static SessionProvider CreateTcpSessionProvider(SessionProviderOptions options,
            OnSessionNotify<SessionCompletedNotify, SessionProviderContext> onSessionNotifyProc)
        {

            SessionProvider provider = new TcpSocketSessionProviderHandle(options, onSessionNotifyProc);

            return provider;
        }

        public static SessionProvider CreateProxySessionProvider(SessionProviderOptions options,
            OnSessionNotify<SessionCompletedNotify, SessionProviderContext> onSessionNotifyProc,
            OnProxyNotify<ProxyNotify> onProxyNotifyProc)
        {

            SessionProvider provider = new TcpProxySessionProviderHandle(options, onSessionNotifyProc, onProxyNotifyProc);

            return provider;
        }

    }
}
