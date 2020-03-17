using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public static class MessagePacketExtension
    {
        public static byte[] BuilderHeadPacket(this byte[] data)
        {
            int defineHeadSize = sizeof(int);
            var builderHeadBytes = new byte[data.Length + defineHeadSize];
            BitConverter.GetBytes(data.Length).CopyTo(builderHeadBytes, 0);
            data.CopyTo(builderHeadBytes, defineHeadSize);

            return builderHeadBytes;
        }
    }
}
