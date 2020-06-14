using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider.Core
{

    public class ProxyProtocolConstructionHelper
    {
        /// <summary>
        /// 从传输数据取出应用数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] TakeHeadAndMessage(byte[] data)
        {
            var bytes = data.Copy(sizeof(long), data.Length - sizeof(long));
            return bytes;
        }

        public static long GetAccessId(byte[] data)
        {
            if (data.IsNull())
                return 0;
            return BitConverter.ToInt64(data, 0);
        }

        public static byte[] WrapAccessId(byte[] data, long accessId)
        {
            var bytes = new byte[data.Length + sizeof(long)];
            BitConverter.GetBytes(accessId).CopyTo(bytes, 0);
            data.CopyTo(bytes, sizeof(long));
            return bytes;
        }
    }
}
