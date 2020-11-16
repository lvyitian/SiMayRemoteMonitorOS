using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class ExecuteFileUpdateSimpleApplication : SimpleApplicationBase
    {
        public async Task UpdateService(SessionProviderContext session, RemoteUpdateKind updateType, byte[] file, string url)
        {
            await CallSimpleService(session, SiMay.Core.MessageHead.S_SIMPLE_SERVICE_UPDATE,
                new RemoteUpdatePacket()
                {
                    UrlOrFileUpdate = updateType,
                    DownloadUrl = updateType == RemoteUpdateKind.Url ? url : string.Empty,
                    FileData = updateType == RemoteUpdateKind.File ? file : new byte[0]
                });
        }
    }
}
