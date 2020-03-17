using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public class MessageDataPacket : EntitySerializerBase
    {
        public long AccessId { get; set; }

        public long DispatcherId { get; set; }

        public byte[] Data { get; set; }
    }
}
