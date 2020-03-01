using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Serialize.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    /// <summary>
    /// 连接分配器
    /// </summary>
    public class ApportionDispatcher : DispatcherBase
    {
        private const Int16 ACK_HEAD = 1000;

        public event Action<ApportionDispatcher, ConnectionWorkType> ApportionTypeHandlerEvent;

        private byte[] _ackRetainData;
        private long _accessId;
        public override void OnMessage()
        {
            var defineHeadSize = sizeof(int);
            if (ListByteBuffer.Count < defineHeadSize)
                return;

            byte[] lenBytes = ListByteBuffer.GetRange(0, defineHeadSize).ToArray();
            int packageLen = BitConverter.ToInt32(lenBytes, 0);

            if (packageLen < 0 || packageLen > ApplicationConfiguartion.Options.MaxPacketSize || packageLen < 25) //数据不合法 或 小于大概ack固定长度
            {
                this.Log(LogOutLevelType.Error, $"Type:{ConnectionWorkType.ToString()} 长度不合法!");
                this.CloseSession();
                return;
            }

            if (packageLen + defineHeadSize > ListByteBuffer.Count)
                return;

            this._ackRetainData = ListByteBuffer.GetRange(defineHeadSize, packageLen).ToArray();
            ListByteBuffer.RemoveRange(0, packageLen + defineHeadSize);

            var longSize = sizeof(long);
            var packageBody = GZipHelper.Decompress(_ackRetainData, longSize, _ackRetainData.Length - longSize);
            var messageHead = TakeMessageHead(packageBody);
            if (messageHead == ACK_HEAD)
            {
                var ack = PacketSerializeHelper.DeserializePacket<AckPacket>(TakeMessage(packageBody));

                this._accessId = ack.AccessId;

                if (ValidityAccessIdWithKey((ConnectionWorkType)ack.Type, ack.AccessId, ack.AccessKey))
                    this.ApportionTypeHandlerEvent?.Invoke(this, (ConnectionWorkType)ack.Type);
                else
                {
                    var midData = MessageHelper.CopyMessageHeadTo(MessageHead.MID_ACCESS_KEY_WRONG);
                    this.CurrentSession.SendAsync(midData.BuilderHeadPacket());
                    this.Log(LogOutLevelType.Debug, $"Type:{((ConnectionWorkType)ack.Type).ToString()} AccessId:{ack.AccessId} 或AccessKey:{ack.AccessKey} 验证失败，登陆不成功!");
                    this.CloseSession();
                }
            }
            else
            {
                this.CloseSession();
                this.Log(LogOutLevelType.Warning, $"未知消息,连接被关闭!");
            }

        }

        /// <summary>
        /// 验证是否允许连接
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool ValidityAccessIdWithKey(ConnectionWorkType type, long id, long key)
        {
            if (type == ConnectionWorkType.MainApplicationConnection && ApplicationConfiguartion.Options.MainApplicationAnonyMous)
                return key.Equals(ApplicationConfiguartion.Options.MainAppAccessKey);//主控端允许匿名Id登陆
            else if (type == ConnectionWorkType.MainApplicationConnection && !ApplicationConfiguartion.Options.MainApplicationAnonyMous)
                return ApplicationConfiguartion.Options.MainApplicationAllowAccessId.Contains(id) && ApplicationConfiguartion.Options.MainAppAccessKey.Equals(key);
            else
            {
                return key.Equals(ApplicationConfiguartion.Options.AccessKey);//其他的暂仅验证AccessKey
            }
        }

        /// <summary>
        /// 获取被控服务AccessId
        /// </summary>
        /// <returns></returns>
        public long GetAccessId()
        {
            if (_ackRetainData.IsNull())
                throw new ArgumentNullException();

            return _accessId;
        }

        /// <summary>
        /// 获取Ack数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetACKPacketData()
        {
            if (_ackRetainData.IsNull())
                throw new ArgumentNullException();

            return _ackRetainData;
        }

        private Int16 TakeMessageHead(byte[] data)
        {
            return BitConverter.ToInt16(data, 0);
        }
        private byte[] TakeMessage(byte[] data)
        {
            var defineHeadSize = sizeof(Int16);
            byte[] payload = new byte[data.Length - defineHeadSize];
            Array.Copy(data, defineHeadSize, payload, 0, data.Length - defineHeadSize);
            return payload;
        }

        public override void OnClosed()
        {
            ListByteBuffer.Clear();
        }
    }
}
