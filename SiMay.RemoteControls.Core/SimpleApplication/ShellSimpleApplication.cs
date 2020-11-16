using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class ShellSimpleApplication : SimpleApplicationBase
    {
        public async Task ExecuteShell(SessionProviderContext session, string cmd)
        {
            await CallSimpleService(session, MessageHead.S_SIMPLE_EXE_SHELL,
                cmd.UnicodeStringToBytes());
        }
    }
}
