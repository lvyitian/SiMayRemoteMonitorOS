using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SiMay.Basic
{
    public class RectangleHelper
    {
        public static bool WhetherContainsInDisplayRectangle(Rectangle containerRect, Rectangle childRect)
        {
            var result = childRect.Y + childRect.Height >= containerRect.Y && childRect.Y <= containerRect.Bottom;
            return result;
        }
    }
}
