using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class MonitorChangePacket : EntitySerializerBase
    {
        public int MonitorIndex { get; set; }
    }
}
