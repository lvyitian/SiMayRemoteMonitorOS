using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class AudioOptionsPacket : EntitySerializerBase
    {
        public int SamplesPerSecond { get; set; }
        public int BitsPerSample { get; set; }
        public int Channels { get; set; }
    }
}
