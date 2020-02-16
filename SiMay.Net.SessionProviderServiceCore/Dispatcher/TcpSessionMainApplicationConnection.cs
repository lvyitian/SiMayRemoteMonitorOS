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
        private long _createdTime = DateTime.Now.ToFileTime();

        public long CreatedTime => _createdTime;

        public override ConnectionWorkType ConnectionWorkType => ConnectionWorkType.MainApplicationConnection;

        private readonly IDictionary<long, TcpSessionChannelDispatcher> _dispatchers;
        public TcpSessionMainApplicationConnection(IDictionary<long, TcpSessionChannelDispatcher> dispatchers)
        {
            _dispatchers = dispatchers;
        }

        public override void OnMessage()
        {
            base.OnMessage();

            int defineHeadSize = sizeof(int);
            do
            {
                if (ListByteBuffer.Count < defineHeadSize)
                    return;

                byte[] lenBytes = ListByteBuffer.GetRange(0, defineHeadSize).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                if (packageLen < 0 || packageLen > ApplicationConfiguartion.Options.MaxPacketSize)
                {
                    this.CloseSession();
                    this.Log(LogOutLevelType.Error, $"Type:{ConnectionWorkType.ToString()} 长度不合法!");
                    return;
                }

                if (packageLen + defineHeadSize > ListByteBuffer.Count)
                    return;

                byte[] data = ListByteBuffer.GetRange(defineHeadSize, packageLen).ToArray();

                this.MessageCompletedHandler(data);
                ListByteBuffer.RemoveRange(0, packageLen + defineHeadSize);

            } while (ListByteBuffer.Count > defineHeadSize);
        }

        private void MessageCompletedHandler(byte[] data)
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

        /// <summary>
        /// 构造带长度消息头的数据
        /// </summary>
        /// <param name="data"></param>
        public override void SendTo(byte[] data)
        {
            base.SendTo(data.BuilderHeadPacket());
        }

        private void TranspondMessage(long dispatcherId, byte[] data)
        {
            TcpSessionChannelDispatcher dispatcher;
            if (_dispatchers.TryGetValue(dispatcherId, out dispatcher))
            {
                dispatcher.SendTo(data.BuilderHeadPacket());//直接转发
            }
            else this.Log(LogOutLevelType.Debug, $"ID:{dispatcherId} 未找到主服务连接!");
        }

        public override void OnClosed()
        {
            ListByteBuffer.Clear();
        }
    }
}
