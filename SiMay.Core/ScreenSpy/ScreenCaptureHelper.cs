using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SiMay.Core.ScreenSpy
{
    /// <summary>
    /// 通用屏幕图像处理类
    /// </summary>
    public class ScreenCaptureHelper
    {
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            System.Int32 dwRop
        );

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("User32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        //[DllImport("user32")]
        //public static extern int GetSystemMetrics(int nIndex);

        //public const int SM_CXSCREEN = 0;// '屏幕宽度
        //public const int SM_CYSCREEN = 1;//'屏幕高度

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC  
                        int nIndex // index of capability  
                        );

        const int HORZRES = 8;
        const int VERTRES = 10;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;

        public static byte[] CaptureNoCursorToBytes(int width, int height)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var img = CaptureNoCursor();
                    if (img == null)
                        throw new Exception("img is null!");
                    SizeImage(img, width, height).Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex.Message + "," + width + "," + height);
                return new byte[0];
            }
        }

        public static Bitmap SizeImage(Image srcImage, int iWidth, int iHeight)
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

        static ImageCodecInfo _ici;
        public static Bitmap KiSaveAsJPEG(Bitmap bmp, int Qty)
        {
            try
            {
                if (_ici == null)
                {
                    foreach (ImageCodecInfo ici in ImageCodecInfo.GetImageEncoders())
                    {
                        if (ici.MimeType.Equals("image/jpeg"))
                        {
                            _ici = ici;
                            break;
                        }
                    }
                    if (_ici == null) return null;
                }

                Bitmap bitmap;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    EncoderParameters ps = new EncoderParameters(1);
                    ps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Qty);

                    bmp.Save(ms, _ici, ps);
                    //SizeImage(bmp, 500, 400).Save(ms, GetCodecInfo("image/jpeg"), ps);

                    bitmap = (Bitmap)Bitmap.FromStream(ms);
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap CaptureNoCursor()
        {
            IntPtr deskDeviceContext = GetWindowDC(GetDesktopWindow());

            Graphics g1 = Graphics.FromHdc(deskDeviceContext);

            //int screenHeight = GetSystemMetrics(SM_CYSCREEN)+100;
            //int screenWidth = GetSystemMetrics(SM_CXSCREEN)+500;

            int screenHeight = GetDeviceCaps(deskDeviceContext, DESKTOPVERTRES);
            int screenWidth = GetDeviceCaps(deskDeviceContext, DESKTOPHORZRES);

            Bitmap MyImage = new Bitmap(screenWidth, screenHeight, g1);

            Graphics g2 = Graphics.FromImage(MyImage);

            IntPtr destdeviceContext = g2.GetHdc();
            IntPtr sourcedeviceContext = g1.GetHdc();

            BitBlt(destdeviceContext, 0, 0, screenWidth, screenHeight, sourcedeviceContext, 0, 0, 13369376);

            //释放dc,否则dc句柄过多会导致程序崩溃
            ReleaseDC(IntPtr.Zero, deskDeviceContext);

            g2.ReleaseHdc(destdeviceContext);
            g1.ReleaseHdc(sourcedeviceContext);

            g1.Dispose();
            g2.Dispose();

            return MyImage;
        }
    }
}
