using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Net.SessionProvider.Providers;

namespace SiMay.Net.SessionProvider
{
    public class SessionProviderFactory
    {
        public static SessionProvider CreateTcpSessionProvider(SessionProviderOptions options)
        {
            SessionProvider provider = new TcpSocketSessionProvider(options);
            return provider;
        }

        public static SessionProvider CreateProxySessionProvider(SessionProviderOptions options)
        {
            SessionProvider provider = new TcpProxySessionProvider(options);
            return provider;
        }

    }
}
