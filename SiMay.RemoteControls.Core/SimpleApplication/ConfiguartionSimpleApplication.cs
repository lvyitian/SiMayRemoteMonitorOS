using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class ConfiguartionSimpleApplication : SimpleApplicationBase
    {
        public async Task SetDescribe(SessionProviderContext session, string describe)
        {
            await CallSimpleService(session, MessageHead.S_SIMPLE_DES, describe);
        }

        public async Task SetGroupName(SessionProviderContext session, string groupName)
        {
            await CallSimpleService(session, MessageHead.S_SIMPLE_GROUP_NAME, groupName);
        }
    }
}
