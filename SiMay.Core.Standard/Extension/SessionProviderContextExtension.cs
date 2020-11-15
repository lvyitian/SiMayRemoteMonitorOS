using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core
{
    public static class SessionProviderContextExtension
    {
        /// <summary>
        /// 发送实体对象
        /// </summary>
        /// <returns></returns>
        public static void SendTo(this SessionProviderContext session, MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            session.SendAsync(bytes);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public static void SendTo(this SessionProviderContext session, MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            session.SendAsync(bytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lpString"></param>
        public static void SendTo(this SessionProviderContext session, MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            session.SendAsync(bytes);
        }

        public static void SendTo(this SessionProviderContext session, byte[] data)
        {
            session.SendAsync(data);
        }

        public static T GetMessageEntity<T>(this SessionProviderContext session)
            where T : new()
            => session.CompletedBuffer.GetMessagePayload().GetMessageEntity<T>();



        public static byte[] GetMessage(this SessionProviderContext session)
            => session.CompletedBuffer.GetMessagePayload();



        public static MessageHead GetMessageHead(this SessionProviderContext session)
            => session.CompletedBuffer.GetMessageHead<MessageHead>();
    }
}
