using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class DesktopViewSimpleApplication : SimpleApplicationBase
    {
        public async Task<Image> GetDesktopViewFrame(SessionProviderContext session, int height, int width)
        {
            var frameResponsd = await CallSimpleService(session, MessageHead.S_SIMPLE_DESKTOPVIEW_REQUEST_FRAME,
                new RequestDesktopViewFramePacket()
                {
                    Height = height,
                    Width = width
                });
            if (!frameResponsd.IsNull() && frameResponsd.IsOK)
            {
                var framePacket = frameResponsd.Datas.GetMessageEntity<ResponseDesktopViewFramePacket>();
                using (MemoryStream ms = new MemoryStream(framePacket.ViewData))
                    return Image.FromStream(ms);
            }
            return null;
        }
    }
}
