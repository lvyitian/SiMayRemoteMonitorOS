using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class AckPack : EntitySerializerBase
    {
        public byte Type { get; set; }
        public long AccessId { get; set; }
        public long AccessKey { get; set; }
    }
}
