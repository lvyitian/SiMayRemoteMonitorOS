using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SiMay.Service.Core
{

    public class AForgeViedo
    {
        private VideoCaptureDevice videoSource;
        private Bitmap _img;

        public Bitmap GetFrame()
        {
            return _img;
        }

        public bool Init()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count <= 0) return false;


            try
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.DesiredFrameSize = videoSource.VideoCapabilities[0].FrameSize;
                videoSource.DesiredFrameRate = videoSource.VideoCapabilities[0].FrameRate;
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
                videoSource.Start();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                _img = eventArgs.Frame.Clone(new Rectangle(0, 0,eventArgs.Frame.Width, eventArgs.Frame.Height), PixelFormat.Format16bppRgb555);

                GC.Collect();
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            catch { }
        }
    }
}