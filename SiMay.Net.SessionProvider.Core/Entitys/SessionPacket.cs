using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public class SessionPacket : EntitySerializerBase
    {
        public SessionItemPacket[] SessionItems { get; set; }
    }

    public class SessionItemPacket : EntitySerializerBase
    {
        public long Id { get; set; }

        public byte[] ACKPacketData { get; set; }
    }
}
