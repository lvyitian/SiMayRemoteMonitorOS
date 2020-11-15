using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Net.SessionProvider;

namespace SiMay.Service.Core
{
    [ServiceName("远程更新服务")]
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_UPDATE)]
    public class RemoteUpdateService : ApplicationRemoteService
    {
        public override void SessionClosed()
        {
            throw new NotImplementedException();
        }

        public override void SessionInited(SessionProviderContext session)
        {
            
        }
    }
}
