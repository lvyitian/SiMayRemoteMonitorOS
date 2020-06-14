using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Sockets.Tcp.TcpConfiguration;

namespace SiMay.Net.SessionProvider
{
    public class SessionProviderFactory
    {
        public static TcpClientSessionProvider CreateTcpClientSessionProvider()
            => CreateTcpClientSessionProvider(new TcpSocketSaeaClientConfiguration());

        public static TcpClientSessionProvider CreateTcpClientSessionProvider(TcpSocketSaeaClientConfiguration clientConfiguration)
        {
            TcpClientSessionProvider provider = new TcpClientSessionProvider(clientConfiguration);
            return provider;
        }

        public static TcpServiceSessionProvider CreateTcpServiceSessionProvider(SessionProviderOptions options)
        {
            TcpServiceSessionProvider provider = new TcpServiceSessionProvider(options);
            return provider;
        }

        public static TcpProxySessionProvider CreateProxySessionProvider(SessionProviderOptions options)
        {
            TcpProxySessionProvider provider = new TcpProxySessionProvider(options);
            return provider;
        }

    }
}
