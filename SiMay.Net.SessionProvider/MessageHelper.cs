using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public class MessageHelper
    {
        public static byte[] BuilderHeadMessage(byte[] body)
        {
            var dataBuilder = new List<byte>();
            dataBuilder.AddRange(BitConverter.GetBytes(body.Length));
            dataBuilder.AddRange(body);
            return dataBuilder.ToArray();
        }
        public static void SendMessage(TcpSocketSaeaSession session, MsgCommand cmd, byte[] body = null)
        {
            if (body == null)
                body = new byte[] { 0 };

            byte[] bytes = new byte[sizeof(Int32) + body.Length + 1];
            BitConverter.GetBytes(body.Length + 1).CopyTo(bytes, 0);
            bytes[4] = (byte)cmd;
            body.CopyTo(bytes, 5);
            session.SendAsync(bytes);
        }
    }
}
