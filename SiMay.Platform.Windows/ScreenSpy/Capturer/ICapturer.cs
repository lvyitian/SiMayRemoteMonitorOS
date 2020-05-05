using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Platform.Windows
{
    public interface ICapturer : IDisposable
    {
        int SelectedScreen { get; set; }
        Bitmap CurrentFrame { get; set; }
        Bitmap PreviousFrame { get; set; }
        Size Size { get; set; }
        Rectangle CurrentScreenBounds { get; }
        int GetScreenCount();
        void Capture();
    }
}
