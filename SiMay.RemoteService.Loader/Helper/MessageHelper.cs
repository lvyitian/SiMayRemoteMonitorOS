using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SiMay.Serialize.Standard.PacketSerializeHelper;

namespace SiMay.RemoteService.Loader
{
    public static class MessageHelper
    {
        /// <summary>
        /// 序列化数据实体，并封装消息头
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo<T>(T cmd, object entity)
            where T : struct
        {
            return CopyMessageHeadTo(cmd, SerializePacket(entity));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo<T>(T cmd, byte[] data, int size)
            where T : struct
        {
            byte[] buff = new byte[size + sizeof(short)];
            BitConverter.GetBytes(Convert.ToInt16(cmd)).CopyTo(buff, 0);
            Array.Copy(data, 0, buff, sizeof(Int16), size);

            return buff;
        }

        /// <summary>
        /// 构建消息至数据头部
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo<T>(T cmd, byte[] data = null)
            where T : struct
        {
            if (data == null)
                data = new byte[] { };

            return CopyMessageHeadTo(cmd, data, data.Length);
        }

        /// <summary>
        /// 获取消息头
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T GetMessageHead<T>(this byte[] data)
            where T : struct
        {
            return (T)Enum.ToObject(typeof(T), BitConverter.ToInt16(data, 0));
        }

        /// <summary>
        /// 获取消息载体
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetMessagePayload(this byte[] data)
        {
            byte[] payload = new byte[data.Length - sizeof(short)];
            Array.Copy(data, sizeof(short), payload, 0, payload.Length);
            return payload;
        }

        /// <summary>
        /// 反序列化数据实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T GetMessageEntity<T>(this byte[] data)
            where T : new()
        {
            try
            {
                var entity = DeserializePacket<T>(GetMessagePayload(data));
                return entity;
            }
            catch
            {
                //File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, $"{typeof(T).FullName}_{DateTime.Now.ToFileTime()}"), data);
            }
            return default;
        }
    }
}
