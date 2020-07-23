using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class ScreenKeyPacket : EntitySerializerBase
    {
        public MOUSEKEY_ENUM Key { get; set; }
        public int Point1 { get; set; }
        public int Point2 { get; set; }
    }
}
