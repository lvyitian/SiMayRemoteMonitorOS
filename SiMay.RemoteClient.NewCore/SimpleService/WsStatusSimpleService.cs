using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public class WsStatusSimpleService : IRemoteSimpleService
    {
        [PacketHandler(MessageHead.S_SIMPLE_SET_SESSION_STATUS)]
        public void SetSystemSession(SessionProviderContext session)
            => SystemHelper.SetWsStatus(session.GetMessage()[0]);
    }
}
