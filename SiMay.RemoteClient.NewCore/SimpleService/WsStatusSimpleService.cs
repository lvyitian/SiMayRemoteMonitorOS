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
    public class WsStatusSimpleService : RemoteSimpleServiceBase
    {
        [PacketHandler(MessageHead.S_SIMPLE_SET_SESSION_STATUS)]
        public void SetWsSession(SessionProviderContext session)
        {
            var state = int.Parse(session.GetMessage().ToUnicodeString());
            SystemHelper.SetWsStatus(state);
        }
    }
}
