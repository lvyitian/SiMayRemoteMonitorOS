using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class MessageBoxSimpleApplication : SimpleApplicationBase
    {
        public async Task MessageBox(SessionProviderContext session, string text, string title, MessageIconKind icon)
        {
            await CallSimpleService(session, SiMay.Core.MessageHead.S_SIMPLE_SERVICE_UPDATE,
                new MessagePacket()
                {
                    MessageTitle = title,
                    MessageBody = text,
                    MessageIcon = (byte)icon
                });
        }
    }
}
