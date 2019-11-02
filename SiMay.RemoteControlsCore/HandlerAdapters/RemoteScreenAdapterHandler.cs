using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.Screen;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteControlsCore.Enum;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class RemoteScreenAdapterHandler : AdapterHandlerBase
    {
        public event Action<RemoteScreenAdapterHandler, string> OnClipoardReceivedEventHandler;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        public void RemoteMouseKeyEvent(MOUSEKEY_ENUM @event, int point1, int point2)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = @event,
                    Point1 = point1,
                    Point2 = point2
                });
        }

        public void RemoteMouseBlock(bool islock)
        {
            byte @lock = islock ? (byte)10 : (byte)11;
            SendAsyncMessage(MessageHead.S_SCREEN_MOUSEBLOCK, new byte[] { @lock });
        }

        public void RemoteScreenBlack()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_BLACKSCREEN);
        }

        public void RemoteChangeScanMode(ScreenScanMode scanMode)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_CHANGESCANMODE, new byte[] { scanMode.ConvertTo<byte>() });
        }

        public void RemoteResetBrandColor(BrandColorMode mode)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_RESET, new byte[] { mode.ConvertTo<byte>() });
        }

        public void RemoteSetScreenQuantity(long qty)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_SETQTY,
                new ScreenSetQtyPack()
                {
                    Quality = qty
                });
        }

        public void SetRemoteClipoardText(string text)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_SET_CLIPBOARD_TEXT,
                                    new ScreenSetClipoardPack()
                                    {
                                        Text = text
                                    });
        }


        public void GetRemoteClipoardText()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_GET_CLIPOARD_TEXT);
        }
        [PacketHandler(MessageHead.C_SCREEN_CLIPOARD_TEXT)]
        private void GetClipoardValueHandler(SessionHandler session)
        {
            var response = session.CompletedBuffer.GetMessageEntity<ScreenClipoardValuePack>();
            this.OnClipoardReceivedEventHandler?.Invoke(this, response.Value);
        }
        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
