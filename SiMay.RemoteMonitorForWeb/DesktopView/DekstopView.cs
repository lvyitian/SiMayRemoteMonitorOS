using SiMay.RemoteControlsCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SiMay.RemoteMonitorForWeb.DesktopView
{
    public class DekstopView : IDesktopView
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string Caption { get; set; }
        public bool InVisbleArea { get; set; }
        public SessionSyncContext SessionSyncContext { get; set; }

        public DekstopView()
        { 
        
        }

        public void CloseDesktopView()
        {
            throw new NotImplementedException();
        }

        public void PlayerDekstopView(Image image)
        {
            throw new NotImplementedException();
        }
    }
}
