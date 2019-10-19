using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider
{
    public class SessionProviderOptions
    {
        public SessionProviderType SessionProviderType { get; set; }

        public int AccessId { get; set; }

        public long AccessKey { get; set; }

        public IPEndPoint ServiceIPEndPoint { get; set; }

        public int PendingConnectionBacklog { get; set; }
    }
}
