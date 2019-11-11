using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SiMay.Core.ScreenSpy
{
    /// <summary>
    /// 通用屏幕图像处理类
    /// </summary>
    public class ImageHelper
    {
        public static Bitmap SizeImage(Image srcImage, Size size)
        {
            try
            {
                Bitmap bitmap = new Bitmap(srcImage, size.Width, size.Height);
                //Graphics g = Graphics.FromImage(bitmap);

                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.DrawImage(srcImage, new Rectangle(0, 0, width, height), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                //g.Dispose();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
