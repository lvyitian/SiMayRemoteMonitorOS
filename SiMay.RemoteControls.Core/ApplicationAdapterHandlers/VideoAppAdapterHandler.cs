using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControls.Core
{
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_VIDEO)]
    public class VideoAppAdapterHandler : ApplicationBaseAdapterHandler
    {
        /// <summary>
        /// 图帧处理事件
        /// </summary>
        public event Action<VideoAppAdapterHandler, Image> OnImageFrameHandlerEvent;

        /// <summary>
        /// 设备未检测到
        /// </summary>
        public event Action<VideoAppAdapterHandler, int> OnCameraNotStartupHandlerEvent;

        [PacketHandler(MessageHead.C_VIEDO_DATA)]
        private void PlayerHandler(SessionProviderContext session)
        {
            var data = session.GetMessage();
            using (MemoryStream ms = new MemoryStream(data))
                OnImageFrameHandlerEvent?.Invoke(this, Image.FromStream(ms));

            SendToAsync(MessageHead.S_VIEDO_GET_DATA);
        }

        [PacketHandler(MessageHead.C_VIEDO_DEVICE_NOTEXIST)]
        private void DeviceNotExistHandler(SessionProviderContext session)
        {
            this.OnCameraNotStartupHandlerEvent?.Invoke(this, 0);
        }

        public void StartGetFrame()
        {
            SendToAsync(MessageHead.S_VIEDO_GET_DATA);
        }

        public void RemoteSetFrameQuantity(int level)
        {
            SendToAsync(MessageHead.S_VIEDO_RESET, new byte[] { (byte)level });
        }
    }
}
