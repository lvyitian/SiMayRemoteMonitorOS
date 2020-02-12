using SiMay.Core.Enums;
using SiMay.Core.ScreenSpy.Entitys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SiMay.Core.ScreenSpy
{
    public class ScreenSpy
    {
        [DllImport("ntdll.dll")]
        public static extern int RtlCompareMemory(
            IntPtr Destination,
            IntPtr Source,
            int Length);

        private ICapturer _capturer;
        public ScreenSpy(ICapturer capturer)
            => _capturer = capturer;
        private int _row = 8;
        private int _column = 8;
        private JpgCompression _jpgCompression = new JpgCompression(50);
        private Rectangle _clientHotRegion = Rectangle.Empty;

        public event Action<Fragment[], DifferStatus> OnDifferencesNotice;

        public ICapturer Capturer
        {
            get
            {
                return _capturer;
            }
        }
        public int ScreenWidth
        {
            get { return _capturer.CurrentScreenBounds.Width; }
        }

        public int ScreenHeight
        {
            get { return _capturer.CurrentScreenBounds.Height; }
        }

        private PixelFormat _format = PixelFormat.Format16bppRgb555;

        public int SetFormat
        {
            set
            {
                switch (value)
                {
                    case 1:
                        _format = PixelFormat.Format1bppIndexed;
                        break;

                    case 4:
                        _format = PixelFormat.Format4bppIndexed;
                        break;

                    case 16:
                        _format = PixelFormat.Format16bppRgb555;
                        break;
                        //case 32:
                        //    _format = PixelFormat.Format32bppPArgb;
                        //    break;
                }
            }
        }

        public long SetImageQuality
        {
            set
            {
                _jpgCompression.SetQuanlity(value);
            }
        }

        public void FindDifferences(bool hotRegionScan, Rectangle rect)
        {
            _capturer.Capture();
            var currenFrame = _capturer.CurrentFrame;
            var previousFrame = _capturer.PreviousFrame;

            if (!hotRegionScan)
                _capturer.Size = new Size(rect.Width, rect.Height);
            else
                _capturer.Size = Size.Empty;

            int currentFrameHeight = currenFrame.Height / _row;
            int currentFrameWidth = currenFrame.Width / _column;

            //余
            int surplusHeight = currenFrame.Height % _row;
            int surplusWidth = currenFrame.Width % _column;

            List<byte> Buffer = new List<byte>();

            try
            {
                int y = 0;
                for (int i = 0; i < _row; i++)
                {
                    int x = 0;
                    for (int j = 0; j < _column; j++)
                    {
                        //计算是否撞热区域
                        int hotRegionX = rect.X + rect.Width;
                        int hotRegionY = rect.Y + rect.Height;
                        bool result = x >= (rect.X - currentFrameWidth) && x <= hotRegionX && y >= (rect.Y - currentFrameHeight) && y <= hotRegionY;
                        if (result || !hotRegionScan)
                        {
                            int sw = 0;
                            int sh = 0;

                            if ((i + 1) == _row)
                                sh = surplusHeight;


                            if ((j + 1) == _column)
                                sw = surplusWidth;

                            int cloneWidth = currentFrameWidth + sw;
                            int cloneHeight = currentFrameHeight + sh;

                            Bitmap m_new = currenFrame.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            Bitmap m_old = previousFrame.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            bool isEqually = BitmapComprae(m_new, m_old);

                            bool isHotRectChanged = hotRegionScan ? (rect.X != _clientHotRegion.X ||
                                    rect.Y != _clientHotRegion.Y ||
                                    rect.Width != _clientHotRegion.Width ||
                                    rect.Height != _clientHotRegion.Height) : false;

                            if (isEqually || isHotRectChanged)
                            {
                                var fragments = new Fragment[] {
                                        new Fragment(){
                                            X = x,
                                            Y = y,
                                            Height = cloneHeight,
                                            Width = cloneWidth,
                                            FragmentData = _jpgCompression.Compress(m_new)
                                        }
                                     };
                                this.OnDifferencesNotice?.Invoke(fragments, DifferStatus.NEXT_SCREEN);
                            }
                            else
                                m_old.Dispose();
                        }
                        x += currentFrameWidth;
                    }
                    y += currentFrameHeight;
                }
            }
            catch { }

            _clientHotRegion = rect;

            this.OnDifferencesNotice?.Invoke(null, DifferStatus.COMPLETED);
        }

        public void FullFindDifferences(bool hotRegionScan, Rectangle rect)
        {
            _capturer.Capture();
            var currenFrame = _capturer.CurrentFrame;
            var previousFrame = _capturer.PreviousFrame;

            if (!hotRegionScan)
                _capturer.Size = new Size(rect.Width, rect.Height);
            else
                _capturer.Size = Size.Empty;

            int currentFrameHeight = currenFrame.Height / _row;
            int currentFrameWidth = currenFrame.Width / _column;

            int surplusHeight = currenFrame.Height % _row;
            int surplusWidth = currenFrame.Width % _column;

            var fragments = new List<Fragment>();

            try
            {
                int y = 0;
                for (int i = 0; i < _row; i++)
                {
                    int x = 0;
                    for (int j = 0; j < _column; j++)
                    {
                        //计算是否撞热区域
                        int hotRegionX = rect.X + rect.Width;
                        int hotRegionY = rect.Y + rect.Height;
                        bool result = x >= (rect.X - currentFrameWidth) && x <= hotRegionX && y >= (rect.Y - currentFrameHeight) && y <= hotRegionY;
                        if (result || !hotRegionScan)//如果是全屏监控就无论是否撞热区域
                        {
                            int sw = 0;
                            int sh = 0;

                            if ((i + 1) == _row)
                                sh = surplusHeight;


                            if ((j + 1) == _column)
                                sw = surplusWidth;

                            int cloneWidth = currentFrameWidth + sw;
                            int cloneHeight = currentFrameHeight + sh;

                            Bitmap m_new = currenFrame.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            Bitmap m_old = previousFrame.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            var isequal = BitmapComprae(m_new, m_old);

                            bool isHotRectChanged = hotRegionScan ? (rect.X != _clientHotRegion.X ||
                                rect.Y != _clientHotRegion.Y ||
                                rect.Width != _clientHotRegion.Width ||
                                rect.Height != _clientHotRegion.Height) : false;

                            if (isequal || isHotRectChanged)
                            {
                                fragments.Add(new Fragment()
                                {
                                    X = x,
                                    Y = y,
                                    Height = cloneHeight,
                                    Width = cloneWidth,
                                    FragmentData = _jpgCompression.Compress(m_new)
                                });
                            }
                            else
                                m_old.Dispose();
                        }

                        x += currentFrameWidth;
                    }
                    y += currentFrameHeight;
                }
            }
            catch { }

            _clientHotRegion = rect;

            if (this.OnDifferencesNotice != null)
            {
                this.OnDifferencesNotice(fragments.ToArray(), DifferStatus.FULL_DIFFERENCES);
                fragments.Clear();
            }
        }

        private Rectangle DiffArea(Bitmap currentFrame, Bitmap previousFrame)
        {
            if (currentFrame.Height != previousFrame.Height || currentFrame.Width != previousFrame.Width)
            {
                throw new Exception("Bitmaps are not of equal dimensions.");
            }
            if (!Bitmap.IsAlphaPixelFormat(currentFrame.PixelFormat) || !Bitmap.IsAlphaPixelFormat(previousFrame.PixelFormat) ||
                !Bitmap.IsCanonicalPixelFormat(currentFrame.PixelFormat) || !Bitmap.IsCanonicalPixelFormat(previousFrame.PixelFormat))
            {
                throw new Exception("Bitmaps must be 32 bits per pixel and contain alpha channel.");
            }
            var width = currentFrame.Width;
            var height = currentFrame.Height;
            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = int.MinValue;
            int bottom = int.MinValue;

            BitmapData bd1 = null;
            BitmapData bd2 = null;

            try
            {
                bd1 = previousFrame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, currentFrame.PixelFormat);
                bd2 = currentFrame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, previousFrame.PixelFormat);

                var bytesPerPixel = Bitmap.GetPixelFormatSize(currentFrame.PixelFormat) / 8;
                var totalSize = bd1.Height * bd1.Width * bytesPerPixel;

                unsafe
                {
                    byte* scan1 = (byte*)bd1.Scan0.ToPointer();
                    byte* scan2 = (byte*)bd2.Scan0.ToPointer();

                    for (int counter = 0; counter < totalSize - bytesPerPixel; counter += bytesPerPixel)
                    {
                        byte* data1 = scan1 + counter;
                        byte* data2 = scan2 + counter;

                        if (data1[0] != data2[0] ||
                            data1[1] != data2[1] ||
                            data1[2] != data2[2] ||
                            data1[3] != data2[3])
                        {
                            // Change was found.
                            var pixel = counter / 4;
                            var row = (int)Math.Floor((double)pixel / bd1.Width);
                            var column = pixel % bd1.Width;
                            if (row < top)
                            {
                                top = row;
                            }
                            if (row > bottom)
                            {
                                bottom = row;
                            }
                            if (column < left)
                            {
                                left = column;
                            }
                            if (column > right)
                            {
                                right = column;
                            }
                        }
                    }
                }

                if (left < right && top < bottom)
                {
                    // Bounding box is valid.  Padding is necessary to prevent artifacts from
                    // moving windows.
                    left = Math.Max(left - 10, 0);
                    top = Math.Max(top - 10, 0);
                    right = Math.Min(right + 10, width);
                    bottom = Math.Min(bottom + 10, height);

                    return new Rectangle(left, top, right - left, bottom - top);
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
            catch
            {
                return Rectangle.Empty;
            }
            finally
            {
                currentFrame.UnlockBits(bd1);
                previousFrame.UnlockBits(bd2);
            }
        }

        private bool BitmapComprae(Bitmap m_new, Bitmap m_old)
        {
            bool isResult = false;

            BitmapData bdNewbmp = m_new.LockBits(
                new Rectangle(0, 0, m_new.Width, m_new.Height),
                ImageLockMode.ReadWrite,
                _format);

            BitmapData bdOldbmp = m_old.LockBits(
                new Rectangle(0, 0, m_old.Width, m_old.Height),
                ImageLockMode.ReadWrite,
                _format);

            int k = RtlCompareMemory(
                bdOldbmp.Scan0,
                bdNewbmp.Scan0,
                bdNewbmp.Stride * m_new.Height);

            m_new.UnlockBits(bdNewbmp);
            m_old.UnlockBits(bdOldbmp);

            if (k < bdNewbmp.Stride * m_old.Height)
            {
                isResult = true;
            }

            return isResult;
        }

        public void Dispose()
        {
            _capturer.Dispose();
        }

        ~ScreenSpy()
        {
            Dispose();
        }
    }
}