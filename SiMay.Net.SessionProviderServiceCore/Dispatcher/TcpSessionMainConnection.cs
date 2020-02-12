using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class TcpSessionMainConnection : TcpSessionChannelDispatcher
    {
        public override ConnectionWorkType ConnectionWorkType => ConnectionWorkType.MainServiceConnection;

        private readonly object _sendLock = new object();
        private readonly IDictionary<long, TcpSessionChannelDispatcher> _dispatchers;
        public TcpSessionMainConnection(IDictionary<long, TcpSessionChannelDispatcher> dispatchers)
        {
            _dispatchers = dispatchers;
        }

        public byte[] ACKPacketData { get; set; }

        long? _accessId;
        int? _packageLength;
        int _transpondOffset;
        public override void OnMessage()
        {
            int defineHeadSize = sizeof(int);
            int defineAccessIdSize = sizeof(long);
            do
            {
                if (!_accessId.HasValue && !_packageLength.HasValue)
                {
                    if (ListByteBuffer.Count < defineHeadSize + defineAccessIdSize)
                        break;

                    byte[] headBytes = ListByteBuffer.GetRange(0, defineHeadSize + defineAccessIdSize).ToArray();
                    this._packageLength = BitConverter.ToInt32(headBytes, 0);
                    this._accessId = BitConverter.ToInt64(headBytes, defineHeadSize);
                    this._transpondOffset = 0;
                }

                TcpSessionChannelDispatcher dispatcher;
                if (_dispatchers.TryGetValue((int)_accessId.Value, out dispatcher) || true)
                {
                    var calculateOffsetLength = (_packageLength.Value + defineHeadSize) - _transpondOffset;
                    if (ListByteBuffer.Count <= calculateOffsetLength)
                        calculateOffsetLength = ListByteBuffer.Count;

                    var data = ListByteBuffer.GetRange(0, calculateOffsetLength).ToArray();

                    dispatcher?.SendTo(MessageHelper.CopyMessageHeadTo(MessageHead.MID_MESSAGE_DATA,
                        new MessageDataPacket()
                        {
                            AccessId = dispatcher.DispatcherId,
                            DispatcherId = this.DispatcherId,
                            Data = data
                        }));

                    this._transpondOffset += data.Length;
                    if (_transpondOffset == _packageLength)
                    {
                        this._accessId = null; this._packageLength = null;
                    }

                    ListByteBuffer.RemoveRange(0, calculateOffsetLength);
                }
            } while (ListByteBuffer.Count > defineHeadSize + defineAccessIdSize);
        }

        public override void SendTo(byte[] data)
        {
            //防止其他线程发送消息
            lock (this._sendLock)
            {
                base.SendTo(data);
            }
        }

        public override void OnClosed()
        {

        }
    }
}
