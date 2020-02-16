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
        long? _lastChannelCreatedTime;
        int? _packageLength;
        int _transpondOffset;
        public override void OnMessage()
        {
            base.OnMessage();

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

                var calculateOffsetLength = (_packageLength.Value + defineHeadSize) - _transpondOffset;
                if (ListByteBuffer.Count <= calculateOffsetLength)
                    calculateOffsetLength = ListByteBuffer.Count;
                this._transpondOffset += calculateOffsetLength;
                if (calculateOffsetLength == 0)
                {
                    this._accessId = null; this._packageLength = null; this._lastChannelCreatedTime = null;continue;
                }
                var data = ListByteBuffer.GetRange(0, calculateOffsetLength).ToArray();
                TcpSessionChannelDispatcher dispatcher;
                if (_dispatchers.TryGetValue(_accessId.Value, out dispatcher) && dispatcher is TcpSessionMainApplicationConnection mainApplicationConnection)
                {
                    if (!_lastChannelCreatedTime.HasValue)
                        this._lastChannelCreatedTime = mainApplicationConnection.CreatedTime;

                    //数据包未完整发送完成期间，如果主控端被登出，则剩余的数据不再发送
                    if (mainApplicationConnection.CreatedTime == _lastChannelCreatedTime.Value)
                    {
                        mainApplicationConnection.SendTo(MessageHelper.CopyMessageHeadTo(MessageHead.MID_MESSAGE_DATA,
                            new MessageDataPacket()
                            {
                                AccessId = dispatcher.DispatcherId,
                                DispatcherId = this.DispatcherId,
                                Data = data
                            }));
                    }
                    else
                    {
                        this.Log(LogOutLevelType.Debug, "原主控端已离线，数据被抛弃!");
                    }
                }
                ListByteBuffer.RemoveRange(0, calculateOffsetLength);
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
            ListByteBuffer.Clear();
        }
    }
}
