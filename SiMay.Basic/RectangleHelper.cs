using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public class RectangleHelper
    {
        public static bool WhetherContainsInRectangle(Rectangle containerRect, Rectangle childRect)
        {
            var result = childRect.Y + childRect.Height >= containerRect.Y && childRect.Y <= containerRect.Bottom;
            return result;
        }
    }
}
