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

        private byte[] _ackRetainPacketData;
        private long _accessId;
        public override void OnMessage()
        {
            var defineHeadSize = sizeof(int);
            if (ListByteBuffer.Count < defineHeadSize)
                return;

            byte[] lenBytes = ListByteBuffer.GetRange(0, defineHeadSize).ToArray();
            int packageLen = BitConverter.ToInt32(lenBytes, 0);

            if (packageLen < 0 || packageLen > ApplicationConfiguartion.MaxPacketSize || packageLen < 25) //数据不合法 或 小于大概ack固定长度
            {
                this.CloseSession();
                return;
            }

            if (packageLen > ListByteBuffer.Count - defineHeadSize)
                return;

            this._ackRetainPacketData = ListByteBuffer.GetRange(0, defineHeadSize + packageLen).ToArray();
            ListByteBuffer.RemoveRange(0, packageLen + defineHeadSize);

            this._accessId = BitConverter.ToInt64(_ackRetainPacketData, defineHeadSize);

            var longSize = sizeof(long);
            var packageBody = GZipHelper.Decompress(_ackRetainPacketData, defineHeadSize + longSize, _ackRetainPacketData.Length - defineHeadSize - longSize);
            var messageHead = TakeMessageHead(packageBody);
            if (messageHead == ACK_HEAD)
            {
                var ack = PacketSerializeHelper.DeserializePacket<AckPacket>(TakeMessage(packageBody));
                if (ValidityAccessIdWithKey(ack.Type, ack.AccessId, ack.AccessKey))
                    this.ApportionTypeHandlerEvent?.Invoke(this, ack.Type);
                else
                    this.CloseSession();
            }
            else
                this.CloseSession();

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
            if (type == ConnectionWorkType.MainApplicationConnection &&
                ApplicationConfiguartion.MainApplicationAnonymous &&
                key.Equals(ApplicationConfiguartion.AccessKey))
                return true;//主控端允许匿名Id登陆

            else if (type == ConnectionWorkType.MainApplicationConnection &&
                !ApplicationConfiguartion.MainApplicationAnonymous &&
                ApplicationConfiguartion.MainApplicationAllowAccessId.Contains(id) &&
                ApplicationConfiguartion.AccessKey.Equals(key))
                return true;
            else
            {
                return key.Equals(ApplicationConfiguartion.AccessKey);//其他的暂仅验证AccessKey
            }
        }

        /// <summary>
        /// 获取被控服务AccessId
        /// </summary>
        /// <returns></returns>
        public long GetAccessId()
        {
            if (_ackRetainPacketData.IsNull())
                throw new ArgumentNullException();

            return _accessId;
        }

        /// <summary>
        /// 获取Ack数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetACKPacketData()
        {
            if (_ackRetainPacketData.IsNull())
                throw new ArgumentNullException();

            return _ackRetainPacketData;
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

        }
    }
}
