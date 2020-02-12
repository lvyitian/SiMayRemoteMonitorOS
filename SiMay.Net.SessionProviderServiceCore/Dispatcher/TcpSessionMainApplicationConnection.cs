using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class TcpSessionMainApplicationConnection : TcpSessionChannelDispatcher
    {
        public override ConnectionWorkType ConnectionWorkType => ConnectionWorkType.MainApplicationConnection;

        private readonly IDictionary<long, TcpSessionChannelDispatcher> _dispatchers;
        public TcpSessionMainApplicationConnection(IDictionary<long, TcpSessionChannelDispatcher> dispatchers)
        {
            _dispatchers = dispatchers;
        }

        public override void OnMessage()
        {
            int defineHeadSize = sizeof(int);
            do
            {
                if (ListByteBuffer.Count < defineHeadSize)
                    return;

                byte[] lenBytes = ListByteBuffer.GetRange(0, defineHeadSize).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                if (packageLen > ListByteBuffer.Count + defineHeadSize)
                    return;

                byte[] data = ListByteBuffer.GetRange(defineHeadSize, packageLen).ToArray();

                this.MessageHandler(data);
                ListByteBuffer.RemoveRange(0, packageLen + defineHeadSize);

            } while (ListByteBuffer.Count > defineHeadSize);
        }

        private void MessageHandler(byte[] data)
        {

            switch (data.GetMessageHead<MessageHead>())
            {
                case MessageHead.APP_PULL_SESSION:
                    this.GetAllSessionItem();
                    break;
                case MessageHead.APP_MESSAGE_DATA:
                    var message = data.GetMessageEntity<MessageDataPacket>();
                    this.TranspondMessage(message.DispatcherId, message.Data);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取所有主服务连接
        /// </summary>
        private void GetAllSessionItem()
        {
            var mainServiceConnections = _dispatchers
                .Select(c => c.Value)
                .Where(c => c.ConnectionWorkType == ConnectionWorkType.MainServiceConnection)
                .Cast<TcpSessionMainConnection>()
                .ToList();

            var data = MessageHelper.CopyMessageHeadTo(MessageHead.MID_SESSION,
                new SessionPacket()
                {
                    SessionItems = mainServiceConnections.Select(c => new SessionItemPacket()
                    {
                        Id = c.DispatcherId,
                        ACKPacketData = c.ACKPacketData
                    }).ToArray()
                });
            SendTo(data);
        }

        private void TranspondMessage(long dispatcherId, byte[] data)
        {
            TcpSessionChannelDispatcher dispatcher;
            if (_dispatchers.TryGetValue(dispatcherId, out dispatcher))
            {
                dispatcher.SendTo(data);//直接转发
            }
        }

        public override void OnClosed()
        {

        }
    }
}
