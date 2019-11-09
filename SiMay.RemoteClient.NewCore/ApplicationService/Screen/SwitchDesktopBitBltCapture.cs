using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.ScreenSpy;
using SiMay.ServiceCore.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.ServiceCore.ApplicationService
{
    public class SwitchDesktopBitBltCapture : ICapturer
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

        private string desktopName = Win32Interop.GetCurrentDesktop();
        private bool _isrun = true;
        private AutoResetEvent @event;
        private AutoResetEvent syncWaitEvent;
        public SwitchDesktopBitBltCapture()
        {
            CurrentFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, PixelFormat);
            PreviousFrame = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height, PixelFormat);
            Graphic = Graphics.FromImage(CurrentFrame);
            //@event = new AutoResetEvent(false);
            //syncWaitEvent = new AutoResetEvent(false);
            //ThreadHelper.CreateThread(() => {
            //    desktopName = Win32Interop.GetCurrentDesktop();
            //    Win32Interop.SwitchToInputDesktop();//切换桌面至当前捕获线程
            //    while (this._isrun)
            //    {
            //        try
            //        {
            //            @event.WaitOne();
            //            lock (_screenLock)
            //            {
            //                var currentDesktopName = Win32Interop.GetCurrentDesktop();
            //                //LogHelper.DebugWriteLog("desktopName :" + currentDesktopName);
            //                if (!desktopName.Equals(currentDesktopName, StringComparison.OrdinalIgnoreCase))
            //                {
            //                    LogHelper.DebugWriteLog("switch desktop:" + currentDesktopName);
            //                    desktopName = currentDesktopName;
            //                    Win32Interop.SwitchToInputDesktop();
            //                    return;
            //                }

            //                PreviousFrame = (Bitmap)CurrentFrame.Clone();
            //                Graphic.CopyFromScreen(CurrentScreenBounds.Left, CurrentScreenBounds.Top, 0, 0, new Size(CurrentScreenBounds.Width, CurrentScreenBounds.Height));
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            LogHelper.WriteErrorByCurrentMethod(ex);
            //        }
            //        finally {
            //            syncWaitEvent.Set();
            //        }
            //    }
            //}, true);
        }

        public void Capture()
        {
            //@event.Set();
            //syncWaitEvent.WaitOne();
            lock (_screenLock)
            {
                //var currentDesktopName = Win32Interop.GetCurrentDesktop();
                ////LogHelper.DebugWriteLog("desktopName :" + currentDesktopName);
                //if (!desktopName.Equals(currentDesktopName, StringComparison.OrdinalIgnoreCase))
                //{
                //    LogHelper.DebugWriteLog("switch desktop:" + currentDesktopName);
                //    desktopName = currentDesktopName;
                //    Win32Interop.SwitchToInputDesktop();
                //    return;
                //}
                Win32Interop.SwitchToInputDesktop();
                PreviousFrame = (Bitmap)CurrentFrame.Clone();
                Graphic.CopyFromScreen(CurrentScreenBounds.Left, CurrentScreenBounds.Top, 0, 0, new Size(CurrentScreenBounds.Width, CurrentScreenBounds.Height));
            }

        }

        public int GetScreenCount()
        {
            return Screen.AllScreens.Length;
        }
        public Rectangle GetVirtualScreenBounds()
        {
            return SystemInformation.VirtualScreen;
        }
        public void Dispose()
        {
            this._isrun = false;
            @event.Set();
            syncWaitEvent.Set();
            Graphic.Dispose();
            CurrentFrame.Dispose();
            PreviousFrame.Dispose();
        }
    }
}
