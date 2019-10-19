using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.ScreenSpy.Entitys
{
    public class Fragment
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public byte[] FragmentData { get; set; }
    }
}
