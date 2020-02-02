using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Serialize
{
    public class PacketSerializeHelper
    {
        public static T DeserializePacket<T>(byte[] data) where T : new()
        {
            T pack = new T();
            var builder = new PacketDeserializeSetup(data, pack);
            return pack;
        }

        public static byte[] SerializePacket<T>(T pack) where T : new()
        {
            var builder = new PacketSerializeSetup(pack);

            return builder.ToArray();
        }
    }
}
