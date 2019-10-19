using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class VideoAppAdapterHandler : AdapterHandlerBase
    {
        /// <summary>
        /// 图帧处理事件
        /// </summary>
        public event Action<VideoAppAdapterHandler, Image> OnImageFrameHandlerEvent;

        /// <summary>
        /// 设备未检测到
        /// </summary>
        public event Action<VideoAppAdapterHandler, int> OnCameraNotStartupHandlerEvent;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        [PacketHandler(MessageHead.C_VIEDO_DATA)]
        private void PlayerHandler(SessionHandler session)
        {
            var data = session.CompletedBuffer.GetMessagePayload();
            using (MemoryStream ms = new MemoryStream(data))
                OnImageFrameHandlerEvent?.Invoke(this, Image.FromStream(ms));

            SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);
        }

        [PacketHandler(MessageHead.C_VIEDO_DEVICE_NOTEXIST)]
        private void DeviceNotExistHandler(SessionHandler session)
        {
            this.OnCameraNotStartupHandlerEvent?.Invoke(this, 0);
        }

        public void StartGetFrame()
        {
            SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);
        }

        public void RemoteSetFrameQuantity(int level)
        {
            SendAsyncMessage(MessageHead.S_VIEDO_RESET, new byte[] { level.ConvertTo<byte>() });
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
