using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public class AckPacket : EntitySerializerBase
    {
        public ConnectionWorkType Type { get; set; }
        public long AccessId { get; set; }
        public long AccessKey { get; set; }
    }
}
