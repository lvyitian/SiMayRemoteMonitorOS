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

        private Bitmap _m_Oldbmp;

        private int _row = 8;
        private int _column = 8;
        private JpgCompression _jpgCompression = new JpgCompression(50);
        private Rectangle _clientHotRegion = new Rectangle(-1, -1, -1, -1);

        public event Action<Fragment[], DifferStatus> OnDifferencesNotice;


        public int ScreenWidth
        {
            get { return _m_Oldbmp.Width; }
        }

        public int ScreenHeight
        {
            get { return _m_Oldbmp.Height; }
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

        public ScreenSpy()
        {
            var map = ScreenCaptureHelper.CaptureNoCursor();
            _m_Oldbmp = new Bitmap(map.Width, map.Height);
        }

        public void FindDifferences(bool hotRegionScan, Rectangle rect)
        {
            Bitmap nBit = ScreenCaptureHelper.CaptureNoCursor();

            if (!hotRegionScan)
                nBit = ScreenCaptureHelper.SizeImage(nBit, rect.Width, rect.Height);

            if (_m_Oldbmp.Height != nBit.Height
                || _m_Oldbmp.Width != nBit.Width)
            {
                _m_Oldbmp.Dispose();
                _m_Oldbmp = new Bitmap(nBit.Width, nBit.Height);
            }

            int newBitHeight = nBit.Height / _row;
            int newBitWidth = nBit.Width / _column;

            //余
            int surplusHeight = nBit.Height % _row;
            int surplusWidth = nBit.Width % _column;

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
                        bool result = x >= (rect.X - newBitWidth) && x <= hotRegionX && y >= (rect.Y - newBitHeight) && y <= hotRegionY;
                        if (result || !hotRegionScan)
                        {
                            int sw = 0;
                            int sh = 0;

                            if ((i + 1) == _row)
                                sh = surplusHeight;


                            if ((j + 1) == _column)
                                sw = surplusWidth;

                            int cloneWidth = newBitWidth + sw;
                            int cloneHeight = newBitHeight + sh;

                            Bitmap m_new = nBit.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            Bitmap m_old = _m_Oldbmp.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            bool isEqually = BitmapComprae(m_new, m_old);

                            bool isHotRectChanged = hotRegionScan ? (rect.X != _clientHotRegion.X ||
                                    rect.Y != _clientHotRegion.Y ||
                                    rect.Width != _clientHotRegion.Width ||
                                    rect.Height != _clientHotRegion.Height) : false;

                            if (isEqually || isHotRectChanged)
                            {
                                //using (MemoryStream ms = new MemoryStream())
                                //{

                                //    m_new.Save(ms, ImageFormat.Jpeg);

                                    var fragments = new Fragment[] {
                                        new Fragment(){
                                            X = x,
                                            Y = y,
                                            Height = cloneHeight,
                                            Width = cloneWidth,
                                            FragmentData = _jpgCompression.Compress(m_new)
                                        }
                                     };
                                    this.OnDifferencesNotice?.Invoke(fragments, DifferStatus.NEXTSCREEN);
                                //}
                            }
                            else
                                m_old.Dispose();
                        }
                        x += newBitWidth;
                    }
                    y += newBitHeight;
                }
            }
            catch { }

            _clientHotRegion = rect;

            this.OnDifferencesNotice?.Invoke(null, DifferStatus.COMPLETE);

            if (_m_Oldbmp != null)
                _m_Oldbmp.Dispose();

            _m_Oldbmp = nBit;
        }

        public void FullFindDifferences(bool hotRegionScan, Rectangle rect)
        {
            Bitmap nBit = ScreenCaptureHelper.CaptureNoCursor();

            if (!hotRegionScan)
                nBit = ScreenCaptureHelper.SizeImage(nBit, rect.Width, rect.Height);

            if (_m_Oldbmp.Height != nBit.Height
                || _m_Oldbmp.Width != nBit.Width)
            {
                _m_Oldbmp.Dispose();
                _m_Oldbmp = new Bitmap(nBit.Width, nBit.Height);
            }

            int newBitHeight = nBit.Height / _row;
            int newBitWidth = nBit.Width / _column;

            int surplusHeight = nBit.Height % _row;
            int surplusWidth = nBit.Width % _column;

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
                        bool result = x >= (rect.X - newBitWidth) && x <= hotRegionX && y >= (rect.Y - newBitHeight) && y <= hotRegionY;
                        if (result || !hotRegionScan)//如果是全屏监控就无论是否撞热区域
                        {
                            int sw = 0;
                            int sh = 0;

                            if ((i + 1) == _row)
                                sh = surplusHeight;


                            if ((j + 1) == _column)
                                sw = surplusWidth;

                            int cloneWidth = newBitWidth + sw;
                            int cloneHeight = newBitHeight + sh;

                            Bitmap m_new = nBit.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            Bitmap m_old = _m_Oldbmp.Clone(
                                new Rectangle(x, y, cloneWidth, cloneHeight),
                                _format);

                            bool isEqually = BitmapComprae(m_new, m_old);

                            bool isHotRectChanged = hotRegionScan ? (rect.X != _clientHotRegion.X ||
                                rect.Y != _clientHotRegion.Y ||
                                rect.Width != _clientHotRegion.Width ||
                                rect.Height != _clientHotRegion.Height) : false;

                            if (isEqually || isHotRectChanged)
                            {
                                //using (MemoryStream ms = new MemoryStream())
                                //{
                                //    m_new.Save(ms, ImageFormat.Jpeg);
                                fragments.Add(new Fragment()
                                {
                                    X = x,
                                    Y = y,
                                    Height = cloneHeight,
                                    Width = cloneWidth,
                                    FragmentData = _jpgCompression.Compress(m_new)
                                });
                                //}
                            }
                            else
                                m_old.Dispose();
                        }

                        x += newBitWidth;
                    }
                    y += newBitHeight;
                }
            }
            catch { }

            _clientHotRegion = rect;

            if (this.OnDifferencesNotice != null)
            {
                this.OnDifferencesNotice(fragments.ToArray(), DifferStatus.FULLDIFFERENCES);
                fragments.Clear();
            }

            if (_m_Oldbmp != null)
                _m_Oldbmp.Dispose();

            _m_Oldbmp = nBit;
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
            _m_Oldbmp.Dispose();
        }

        ~ScreenSpy()
        {
            Dispose();
        }
    }
}