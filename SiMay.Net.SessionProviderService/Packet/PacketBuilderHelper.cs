using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Package
{
    public class PacketBuilderHelper
    {
        public static T BuilderPacket<T>(byte[] data) where T : new()
        {
            T pack = new T();
            var builder = new PackDeserializeSetup(data, pack);
            return pack;
        }

        public static byte[] BuilderPacketBytes<T>(T pack) where T : new()
        {
            var builder = new PackSerializeSetup(pack);

            return builder.ToArray();
        }
    }
}
