using SiMay.Core.ScreenSpy;
using SiMay.ServiceCore.ApplicationService;
using SiMay.ServiceCore.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore.Helper
{
    public class ImageExtensionHelper
    {
        static ICapturer _capturer = new BitBltCapture(false);
        static bool _hasSystemAuthor = AppConfiguartion.HasSystemAuthority.Equals("true", StringComparison.OrdinalIgnoreCase);
        public static byte[] CaptureNoCursorToBytes(Size size)
        {
            if (_hasSystemAuthor)
                Win32Interop.SwitchToInputDesktop();

            _capturer.Size = size;
            _capturer.Capture();
            using (var ms = new MemoryStream())
            {
                _capturer.CurrentFrame.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
