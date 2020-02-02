using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class AudioOptionsPack : EntitySerializerBase
    {
        public int SamplesPerSecond { get; set; }
        public int BitsPerSample { get; set; }
        public int Channels { get; set; }
    }
}
