using Newtonsoft.Json;
using SiMay.Basic;
using SiMay.RemoteControlsCore;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public class DesktopView : IDesktopView
    {
        public string Id { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Caption { get; set; }
        public bool InVisbleArea { get; set; }
        public SessionSyncContext SessionSyncContext { get; set; }
        public MainApplicationAdapterHandler Owner { get; set; }

        public WebSocketSession Session { get; set; }

        public DesktopView()
        {
            var desktopId = "A" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            this.Id = desktopId;
        }

        public void CloseDesktopView()
        {
            SessionSyncContext = null;
            Owner = null;
            Session = null;
        }

        public void PlayerDekstopView(Image image)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var id = SessionSyncContext.KeyDictions["DesktopViewId"].ToString();
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
