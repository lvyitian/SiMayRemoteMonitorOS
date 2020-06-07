using SiMay.Basic;
using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Core
{

    public class SysMessageConstructionHelper
    {
        public static T GetMessageEntity<T>(byte[] data)
            where T : new()
        {
            return TakeHeadAndMessage(data).GetMessageEntity<T>();
        }

        public static byte[] GetMessage(byte[] data)
        {
            return TakeHeadAndMessage(data).GetMessagePayload();
        }

        public static MessageHead GetMessageHead(byte[] data)
        {
            return TakeHeadAndMessage(data).GetMessageHead<MessageHead>();
        }

        /// <summary>
        /// 从传输数据取出应用数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] TakeHeadAndMessage(byte[] data)
        {
            var bytes = data.Copy(sizeof(long), data.Length - sizeof(long));
            return GZipHelper.Decompress(bytes);
        }

        public static long GetAccessId(byte[] data)
        {
            if (data.IsNull())
                return 0;
            return BitConverter.ToInt64(data, 0);
        }

        public static byte[] WrapAccessId(byte[] data, long accessId)
        {
            var compressData = GZipHelper.Compress(data, 0, data.Length);
            var bytes = new byte[compressData.Length + sizeof(long)];
            BitConverter.GetBytes(accessId).CopyTo(bytes, 0);
            compressData.CopyTo(bytes, sizeof(long));
            return bytes;
        }

        //public static byte[] CopyMessageHeadTo(MessageHead msg, object entity)
        //{
        //    byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
        //    return EncapSysIntervalAccessId(bytes);
        //}
        //public static byte[] CopyMessageHeadTo(MessageHead msg, byte[] data = null)
        //{
        //    byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
        //    return WrapAccessIdInterval(session, bytes);
        //}
        //public static byte[] CopyMessageHeadTo(MessageHead msg, string lpString)
        //{
        //    byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
        //    SendToBefore(session, bytes);
        //}

        //private static byte[] EncapSysIntervalAccessId(byte[] data)
        //{

        //}
    }
}
