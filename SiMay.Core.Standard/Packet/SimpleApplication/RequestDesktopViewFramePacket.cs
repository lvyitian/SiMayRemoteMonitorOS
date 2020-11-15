using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class RequestDesktopViewFramePacket : EntitySerializerBase
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class ResponseDesktopViewFramePacket : EntitySerializerBase
    {
        public byte[] ViewData { get; set; }
    }
}
