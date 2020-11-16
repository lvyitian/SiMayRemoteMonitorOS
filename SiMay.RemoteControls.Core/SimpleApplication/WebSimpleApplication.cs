using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class WebSimpleApplication : SimpleApplicationBase
    {
        public async Task<HttpDownloadTaskItemContext> JoinHttpDownload(SessionProviderContext session, string fileName, string url)
        {
            var responsd = await CallSimpleService(session, MessageHead.S_SIMPLE_JOIN_HTTP_DOWNLOAD,
                new JoinHttpDownloadPacket()
                {
                    FileName = fileName,
                    Url = url
                });
            if (!responsd.IsNull() && responsd.IsOK)
            {
                return responsd.Datas.GetMessageEntity<HttpDownloadTaskItemContext>();
            }

            return null;
        }

        public async Task SetHttpDownloadStatus(SessionProviderContext session, string id)
        {
            await CallSimpleService(session, MessageHead.S_SIMPLE_SET_HTTP_DOWNLOAD_STATUS, id);
        }

        public async Task<HttpDownloadTaskItemContext[]> GetHttpDownloadStatusContexts(SessionProviderContext session)
        {
            var responsd = await CallSimpleService(session, MessageHead.S_SIMPLE_HTTP_DOWNLOAD_STATUS_LIST);
            if (!responsd.IsNull() && responsd.IsOK)
            {
                return responsd.Datas.GetMessageEntity<HttpDownloadStatusList>().httpDownloadTaskItemContexts;
            }

            return null;
        }
    }
}
