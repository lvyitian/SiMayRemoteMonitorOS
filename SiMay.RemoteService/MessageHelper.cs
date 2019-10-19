using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService
{
    public class MsgHelper
    {
        public static short GetMessageHead(byte[] data)
        {
            return BitConverter.ToInt16(data, 0);
        }
        public static byte[] GetMessagePayload(byte[] data)
        {
            byte[] payload = new byte[data.Length - sizeof(short)];
            Array.Copy(data, sizeof(short), payload, 0, payload.Length);
            return payload;
        }
        public static void SendMessage(TcpSocketSaeaSession session, short messageHead, byte[] payload)
        {
            byte[] pack = new byte[payload.Length + sizeof(short)];
            BitConverter.GetBytes(messageHead).CopyTo(pack, 0);
            Array.Copy(payload, 0, pack, sizeof(short), payload.Length);
            session?.SendAsync(pack);
        }
    }
}
