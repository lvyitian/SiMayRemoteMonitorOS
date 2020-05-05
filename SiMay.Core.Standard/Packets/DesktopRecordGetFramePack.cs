using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class DesktopRecordGetFramePack : EntitySerializerBase
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int TimeSpan { get; set; }
    }
}
