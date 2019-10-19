using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class MessageHelper
    {
        public static byte[] CommandCopyTo(MsgCommand cmd, byte[] data)
        {
            byte[] bytes = new byte[data.Length + 1];
            bytes[0] = (byte)cmd;
            data.CopyTo(bytes, 1);

            return bytes;
        }

        public static byte[] CommandCopyTo(MsgCommand cmd)
        {
            return new byte[] { (byte)cmd };
        }
    }
}
