using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitorForWeb.DesktopView;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public class DesktopViewContextsHelper
    {
        public static int Height { get; set; }

        public static int Width { get; set; }

        public static IDesktopView CreateDesktopView(SessionSyncContext syncContext, WebSocketSession session)
        {
            var desktopView = new DekstopView
            {
                Height = Height,
                Width = Width,
                Session = session,
                SessionSyncContext = syncContext
            };
            return desktopView;

        }
    }
}
