using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public class DesktopViewSimpleService : IRemoteSimpleService
    {
        [PacketHandler(MessageHead.S_MAIN_DESKTOPVIEW_GETFRAME)]
        public ResponseDesktopViewFramePacket SendNextFrameDesktopView(SessionProviderContext session)
        {
            var request = session.GetMessageEntity<RequestDesktopViewFramePacket>();
            if (request.Width == 0 || request.Height == 0)
                return null;

            var viewData = ImageExtensionHelper.CaptureNoCursorToBytes(new Size(request.Width, request.Height));

            return new ResponseDesktopViewFramePacket()
            {
                ViewData = viewData
            };
        }
    }
}
