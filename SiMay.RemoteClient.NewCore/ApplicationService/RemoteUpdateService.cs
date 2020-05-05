using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.ServiceCore.Attributes;
using SiMay.Sockets.Tcp.Session;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("远程更新服务")]
    [ServiceKey(AppJobConstant.REMOTE_UPDATE)]
    public class RemoteUpdateService : ApplicationRemoteService
    {
        public override void SessionClosed()
        {
            throw new NotImplementedException();
        }

        public override void SessionInited(TcpSocketSaeaSession session)
        {
            
        }
    }
}
