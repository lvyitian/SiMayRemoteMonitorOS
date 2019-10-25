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
using static SiMay.ServiceCore.Win32Api;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("远程桌面")]
    [ServiceKey("RemoteDesktopJob")]
    public class ScreenService : ServiceManager, IApplicationService
    {

        private int _bscanmode = 1; //0逐行 1差异
        private ScreenSpy _spy;
        private PacketModelBinder<TcpSocketSaeaSession> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession>();
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
                    this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case TcpSocketCompletionNotify.OnClosed:
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

            _spy = new ScreenSpy();
            _spy.OnDifferencesNotice += ScreenDifferences_OnDifferencesNotice;

            this.SendDesktopBitInfo();
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

        private void SendDesktopBitInfo()
        {
            SendAsyncToServer(MessageHead.C_SCREEN_BITINFO,
               new ScreenInitBitPack()
               {
                   Height = _spy.ScreenHeight,
                   Width = _spy.ScreenWidth
               });
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

            if (_bscanmode == 0)
                _spy.FindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            else if (_bscanmode == 1)
                _spy.FullFindDifferences(ishotRegtionScan, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
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
            int p1 = @event.Point1;
            int p2 = @event.Point2;
            switch (@event.Key)
            {
                case MOUSEKEY_ENUM.Move:
                    SetCursorPos(p1, p2);
                    break;

                case MOUSEKEY_ENUM.LeftDown:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.LeftUp:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.MiddleDown:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.MiddleUp:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.RightDown:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.RightUp:
                    SetCursorPos(p1, p2);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.Wheel:
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, p1, 0);
                    break;

                case MOUSEKEY_ENUM.KeyDown:
                    keybd_event((byte)p1, 0, 0, 0);
                    break;

                case MOUSEKEY_ENUM.KeyUp:
                    keybd_event((byte)p1, 0, WM_KEYUP, 0);
                    break;
            }
        }
    }
}