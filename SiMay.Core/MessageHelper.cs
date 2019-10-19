using SiMay.Core.Extensions;
using System;
using static SiMay.Serialize.PacketSerializeHelper;

namespace SiMay.Core
{

    public static class MessageHelper
    {
        /// <summary>
        /// 序列化数据实体，并封装消息头
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo(MessageHead cmd, object entity)
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
        public static byte[] CopyMessageHeadTo(MessageHead cmd, byte[] data, int size)
        {
            byte[] buff = new byte[size + sizeof(short)];
            BitConverter.GetBytes((short)cmd).CopyTo(buff, 0);
            Array.Copy(data, 0, buff, sizeof(short), size);

            return buff;
        }

        /// <summary>
        /// 构建消息至数据头部
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo(MessageHead cmd, byte[] data = null)
        {
            if (data == null)
                data = new byte[] { };

            return CopyMessageHeadTo(cmd, data, data.Length);
        }

        /// <summary>
        /// 构建消息头至数据头部
        /// </summary>
        /// <param name="cmd">消息头</param>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static byte[] CopyMessageHeadTo(MessageHead cmd, string str)
        {
            byte[] data = str.UnicodeStringToBytes();

            return CopyMessageHeadTo(cmd, data, data.Length);
        }

        /// <summary>
        /// 获取消息头
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static MessageHead GetMessageHead(this byte[] data)
            => (MessageHead)BitConverter.ToInt16(data, 0);

        /// <summary>
        /// 获取消息载体
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetMessagePayload(this byte[] data)
        {
            byte[] bytes = new byte[data.Length - sizeof(short)];
            Array.Copy(data, sizeof(short), bytes, 0, bytes.Length);
            return bytes;
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
            var entity = DeserializePacket<T>(GetMessagePayload(data));
            return entity;
        }
    }
}