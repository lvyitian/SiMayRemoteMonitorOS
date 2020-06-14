using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class DesktopViewGetFramePack : EntitySerializerBase
    {
        public int Height { get; set; }
        public int Width { get; set; }
        //public int TimeSpan { get; set; }
        //public bool InVisbleArea { get; set; }
    }

    public class DesktopViewFramePack : EntitySerializerBase
    {
        //public bool InVisbleArea { get; set; }
        public byte[] ViewData { get; set; }
    }
}
