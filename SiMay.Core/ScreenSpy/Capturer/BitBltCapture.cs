using SiMay.Core;
using SiMay.Core.ScreenSpy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.Core.ScreenSpy
{
    public class BitBltCapture : ICapturer
    {
        public Bitmap CurrentFrame { get; set; }
        public Rectangle CurrentScreenBounds { get; set; } = Screen.PrimaryScreen.Bounds;
        public bool IsCapturing { get; set; }
        public int PauseForMilliseconds { get; set; }
        public Bitmap PreviousFrame { get; set; }
        public int SelectedScreen { get; private set; } = Screen.AllScreens.ToList().IndexOf(Screen.PrimaryScreen);
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
        public PixelFormat PixelFormat
        {
            get
            {
                return _pixelFormat;
            }
            set
            {
                _pixelFormat = value;
                //后续解决
                //lock (_screenLock)
                //{
                //    if (value == _pixelFormat)
                //        return;
                //    _pixelFormat = value;

                //    ResizeImage();

                //    _originBitmap = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, _pixelFormat);
                //    Graphic = Graphics.FromImage(_originBitmap);
                //}
            }
        }
        private Graphics Graphic { get; set; }


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
                    Graphic.CopyFromScreen(CurrentScreenBounds.Left, CurrentScreenBounds.Top, 0, 0, new Size(CurrentScreenBounds.Width, CurrentScreenBounds.Height));


                    if (!_imageSize.IsEmpty)
                        CurrentFrame = new Bitmap(_originBitmap, _imageSize);
                    else
                        CurrentFrame = _originBitmap;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
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
            _isRetainPreviousFrame = isRetainPreviousFrame;
            _imageSize = imageSize;

            ResizeImage();

            _originBitmap = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, _pixelFormat);
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
