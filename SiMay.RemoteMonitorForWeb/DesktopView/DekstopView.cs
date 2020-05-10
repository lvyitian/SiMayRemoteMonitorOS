using Newtonsoft.Json;
using SiMay.Basic;
using SiMay.RemoteControlsCore;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public MainApplicationAdapterHandler Owner { get; set; }

        public WebSocketSession Session { get; set; }

        public void CloseDesktopView()
        {
            throw new NotImplementedException();
        }

        public void PlayerDekstopView(Image image)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var id = SessionSyncContext.KeyDictions["Id"].ToString();
                Session.Send(JsonConvert.SerializeObject(
                        new
                        {
                            desktopId = id,
                            code = WebMessageHead.S_DESKTOP_VIEW_DATA,
                            imageBase64 = Convert.ToBase64String(ms.ToArray())

                        }));
            }
        }
    }
}
