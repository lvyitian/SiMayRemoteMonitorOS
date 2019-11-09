using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.Screen;
using SiMay.Core.ScreenSpy.Entitys;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteControlsCore.Enum;
using static SiMay.Serialize.PacketSerializeHelper;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class RemoteScreenAdapterHandler : AdapterHandlerBase
    {

        /// <summary>
        /// 远程屏幕服务初始化完成
        /// </summary>
        public event Action<RemoteScreenAdapterHandler, int, int> OnServcieInitEventHandler;

        /// <summary>
        /// 获取剪切板
        /// </summary>
        public event Action<RemoteScreenAdapterHandler, string> OnClipoardReceivedEventHandler;

        /// <summary>
        /// 屏幕帧处理
        /// </summary>
        public event Action<RemoteScreenAdapterHandler, Fragment[], ScreenReceivedType> OnScreenFragmentEventHandler;

        //已接收帧数
        private int _frameCount = 0;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        public void RemoteMouseKeyEvent(MOUSEKEY_ENUM @event, int point1, int point2)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = @event,
                    Point1 = point1,
                    Point2 = point2
                });
        }

        [PacketHandler(MessageHead.C_SCREEN_BITINFO)]
        public void SetBitmapHandler(SessionHandler session)
        {
            var bitinfo = session.CompletedBuffer.GetMessageEntity<ScreenInitBitPack>();
            this.OnServcieInitEventHandler?.Invoke(this, bitinfo.Height, bitinfo.Width);
        }

        [PacketHandler(MessageHead.C_SCREEN_DIFFBITMAP)]
        public void FullFragmentHandler(SessionHandler session)
        {
            var fragments = session.CompletedBuffer.GetMessageEntity<ScreenFragmentPack>();
            this.OnScreenFragmentEventHandler?.Invoke(this, fragments.Fragments, ScreenReceivedType.Noninterlaced);
        }

        [PacketHandler(MessageHead.C_SCREEN_BITMP)]
        public void SigleFragmentHandler(SessionHandler session)
        {
            int dataSize = session.CompletedBuffer.Length;
            var fragments = session.CompletedBuffer.GetMessageEntity<ScreenFragmentPack>();
            this.OnScreenFragmentEventHandler?.Invoke(this, fragments.Fragments, ScreenReceivedType.Difference);
        }
        [PacketHandler(MessageHead.C_SCREEN_SCANCOMPLETE)]
        public void ScanFinishHandler(SessionHandler session)
        {
            this.OnScreenFragmentEventHandler?.Invoke(this, new Fragment[0], ScreenReceivedType.DifferenceEnd);
        }

        public void StartGetScreen(int height, int width, int x, int y, ScreenDisplayMode mode)
        {
            var rect = SerializePacket(new ScreenHotRectanglePack()
            {
                X = x,
                Y = y,
                Height = height,
                Width = width,
                CtrlMode = mode.ConvertTo<int>()
            });
            //第一帧不计入连续帧
            for (int i = 0; i < 3; i++)
                SendAsyncMessage(MessageHead.S_SCREEN_NEXT_SCREENBITMP, rect);
        }

        public void GetNextScreen(int height, int width, int x, int y, ScreenDisplayMode mode)
        {
            if (this.IsClose)
                return;

            _frameCount++;
            //Console.WriteLine(this.imgDesktop.Height + " | " + Width + "|" + this.Height + " | " + this.Width);

            if (_frameCount == 1)//使帧数更连续
            {
                var rect = SerializePacket(new ScreenHotRectanglePack()
                {
                    X = x,
                    Y = y,
                    Height = height,
                    Width = width,
                    CtrlMode = mode.ConvertTo<int>()
                });
                for (int i = 0; i < 3; i++)
                    SendAsyncMessage(MessageHead.S_SCREEN_NEXT_SCREENBITMP, rect);
            }
            else if (_frameCount == 3)
                _frameCount = 0;
        }

        public void GetInitializeBitInfo()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_GET_INIT_BITINFO);
        }

        public void RemoteMouseBlock(bool islock)
        {
            byte @lock = islock ? (byte)10 : (byte)11;
            SendAsyncMessage(MessageHead.S_SCREEN_MOUSEBLOCK, new byte[] { @lock });
        }

        public void RemoteScreenBlack()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_BLACKSCREEN);
        }

        public void RemoteChangeScanMode(ScreenScanMode scanMode)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_CHANGESCANMODE, new byte[] { scanMode.ConvertTo<byte>() });
        }

        public void RemoteResetBrandColor(BrandColorMode mode)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_RESET, new byte[] { mode.ConvertTo<byte>() });
        }

        public void RemoteSetScreenQuantity(long qty)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_SETQTY,
                new ScreenSetQtyPack()
                {
                    Quality = qty
                });
        }

        public void SetRemoteClipoardText(string text)
        {
            SendAsyncMessage(MessageHead.S_SCREEN_SET_CLIPBOARD_TEXT,
                                    new ScreenSetClipoardPack()
                                    {
                                        Text = text
                                    });
        }
        public void SendCtrlAltDel()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_CTRL_ALT_DEL);
        }

        public void GetRemoteClipoardText()
        {
            SendAsyncMessage(MessageHead.S_SCREEN_GET_CLIPOARD_TEXT);
        }
        [PacketHandler(MessageHead.C_SCREEN_CLIPOARD_TEXT)]
        private void GetClipoardValueHandler(SessionHandler session)
        {
            var response = session.CompletedBuffer.GetMessageEntity<ScreenClipoardValuePack>();
            this.OnClipoardReceivedEventHandler?.Invoke(this, response.Value);
        }
        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
