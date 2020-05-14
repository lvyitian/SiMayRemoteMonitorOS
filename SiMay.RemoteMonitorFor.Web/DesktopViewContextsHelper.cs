using SiMay.RemoteControlsCore;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public class DesktopViewContextsHelper
    {
        public static int Height { get; set; }

        public static int Width { get; set; }

        public static DesktopView CreateDesktopView(SessionSyncContext syncContext, WebSocketSession session)
        {
            var desktopView = new DesktopView
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
