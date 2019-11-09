using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Core.ScreenSpy
{
    public interface ICapturer : IDisposable
    {
        int SelectedScreen { get; }
        Bitmap CurrentFrame { get; set; }
        Bitmap PreviousFrame { get; set; }
        PixelFormat PixelFormat { get; set; }
        Rectangle CurrentScreenBounds { get; }
        int GetScreenCount();
        void Capture();
    }
}
