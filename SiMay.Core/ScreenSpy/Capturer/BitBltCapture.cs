using SiMay.Core;
using SiMay.Core.ScreenSpy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SiMay.Core.ScreenSpy
{
    public class BitBltCapture : ICapturer
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC  
                        int nIndex // index of capability  
                        );

        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;

        public Bitmap CurrentFrame { get; set; }
        public Rectangle CurrentScreenBounds { get; set; }
        public bool IsCapturing { get; set; }
        public int PauseForMilliseconds { get; set; }
        public Bitmap PreviousFrame { get; set; }
        public int SelectedScreen
        {
            get
            {
                return _selectedScreen;
            }
            set
            {
                var screen = Screen.AllScreens[value];
                CurrentScreenBounds = this.GetScreenRectangle(screen);
                _selectedScreen = value;
            }
        }
        public Size Size
        {
            get
            {
                return _imageSize;
            }
            set
            {
                lock (_screenLock)
                {
                    if (value == _imageSize)
                        return;

                    _imageSize = value;

                    //为了保证安全,重new
                    ResizeImage();
                }
            }
        }

        private PixelFormat _pixelFormat = PixelFormat.Format16bppRgb555;
        private bool _isRetainPreviousFrame;
        private Size _imageSize = Size.Empty;
        private int _selectedScreen = Screen.AllScreens.ToList().IndexOf(Screen.PrimaryScreen);
        private Graphics Graphic { get; set; }
        private Size DesktopSize
        {
            get
            {
                var hdc = GetDC(IntPtr.Zero);
                int screenHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);
                int screenWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
                return new Size(screenWidth, screenHeight);
            }
        }
        private object _screenLock = new object();
        private Bitmap _originBitmap;
        public void Capture()
        {
            try
            {
                lock (_screenLock)
                {
                    if (_isRetainPreviousFrame)
                        PreviousFrame = (Bitmap)CurrentFrame.Clone();

                    Graphic.CopyFromScreen(CurrentScreenBounds.X, CurrentScreenBounds.Y, 0, 0, new Size(DesktopSize.Width, DesktopSize.Height));
                    if (!_imageSize.IsEmpty)
                    {
                        if (_imageSize.Width >= _originBitmap.Width || _imageSize.Height >= _originBitmap.Height)
                            CurrentFrame = _originBitmap;
                        else
                            CurrentFrame = new Bitmap(_originBitmap, _imageSize);
                    }
                    else
                        CurrentFrame = _originBitmap;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
        }

        private Rectangle GetScreenRectangle(Screen screen)
        {
            var bounds = screen.Bounds;
            return new Rectangle(new Point(bounds.X, bounds.Y), DesktopSize);
        }

        public void Dispose()
        {
            if (_isRetainPreviousFrame)
                PreviousFrame.Dispose();

            Graphic.Dispose();
            CurrentFrame.Dispose();
            _originBitmap.Dispose();
        }

        public int GetScreenCount()
        {
            return Screen.AllScreens.Length;
        }

        public Rectangle GetVirtualScreenBounds()
        {
            return SystemInformation.VirtualScreen;
        }

        public BitBltCapture(bool isRetainPreviousFrame)
            : this(isRetainPreviousFrame, Size.Empty)
        {

        }
        public BitBltCapture(bool isRetainPreviousFrame, Size imageSize)
        {
            CurrentScreenBounds = GetScreenRectangle(Screen.PrimaryScreen);
            _isRetainPreviousFrame = isRetainPreviousFrame;
            _imageSize = imageSize;

            ResizeImage();

            _originBitmap = new Bitmap(DesktopSize.Width, DesktopSize.Height, _pixelFormat);
            Graphic = Graphics.FromImage(_originBitmap);
        }

        private void ResizeImage()
        {
            var height = _imageSize.IsEmpty ? CurrentScreenBounds.Height : _imageSize.Height;
            var width = _imageSize.IsEmpty ? CurrentScreenBounds.Width : _imageSize.Width;

            if (_isRetainPreviousFrame)
                PreviousFrame = new Bitmap(width, height, _pixelFormat);
            CurrentFrame = new Bitmap(width, height, _pixelFormat);
        }
    }
}
