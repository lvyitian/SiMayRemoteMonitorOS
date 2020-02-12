using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public class SessionClosedPacket : EntitySerializerBase
    {
        public long Id { get; set; }
    }
}
