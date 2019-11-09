using SiMay.Core;
using SiMay.Core.ScreenSpy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.ServiceCore.ApplicationService
{
    public class BitBltCapture : ICapturer
    {
        public Bitmap CurrentFrame { get; set; }
        public Rectangle CurrentScreenBounds { get; set; } = Screen.PrimaryScreen.Bounds;
        public bool IsCapturing { get; set; }
        public int PauseForMilliseconds { get; set; }
        public Bitmap PreviousFrame { get; set; }
        public int SelectedScreen { get; private set; } = Screen.AllScreens.ToList().IndexOf(Screen.PrimaryScreen);

        private PixelFormat _pixelFormat = PixelFormat.Format32bppPArgb;
        public PixelFormat PixelFormat
        {
            get
            {
                return _pixelFormat;
            }
            set
            {
                lock (_screenLock)
                {
                    if (value == _pixelFormat)
                        return;
                    CurrentFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, value);
                    PreviousFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, value);
                    _pixelFormat = value;
                }
            }
        }
        private Graphics Graphic { get; set; }


        private object _screenLock = new object();

        public void Capture()
        {
            try
            {
                lock (_screenLock)
                {
                    PreviousFrame = (Bitmap)CurrentFrame.Clone();
                    Graphic.CopyFromScreen(CurrentScreenBounds.Left, CurrentScreenBounds.Top, 0, 0, new Size(CurrentScreenBounds.Width, CurrentScreenBounds.Height));
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
        }

        public void Dispose()
        {
            Graphic.Dispose();
            CurrentFrame.Dispose();
            PreviousFrame.Dispose();
        }

        public int GetScreenCount()
        {
            return Screen.AllScreens.Length;
        }

        public Rectangle GetVirtualScreenBounds()
        {
            return SystemInformation.VirtualScreen;
        }

        public BitBltCapture()
        {
            CurrentFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, PixelFormat);
            PreviousFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, PixelFormat);
            Graphic = Graphics.FromImage(CurrentFrame);
        }
    }
}
