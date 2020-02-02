using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Net.SessionProviderService.Delagate;
using SiMay.Net.SessionProviderService.Notify;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class TcpSessionChannelContext
    {
        const short AckPacket = 1000;

        /// <summary>
        /// 关联的会话(通常是主控工作连接)
        /// </summary>
        private TcpSocketSaeaSession _joinSession;
        public TcpSocketSaeaSession JoinSession
        {
            get { return _joinSession; }
        }

        /// <summary>
        /// 通道会话(通常是服务工作连接)
        /// </summary>
        private TcpSocketSaeaSession _session;
        public TcpSocketSaeaSession Session
        {
            get { return _session; }
        }

        /// <summary>
        /// 通道数据缓冲区
        /// </summary>
        private List<byte> _buffer = new List<byte>();
        public byte[] GetBuffer
        {
            get { return this._buffer.ToArray(); }
        }

        /// <summary>
        /// 通道服务类型(主控连接，服务主连接，服务工作连接，主控工作连接)
        /// </summary>
        private TcpChannelContextServiceType _channelType = TcpChannelContextServiceType.None;
        public TcpChannelContextServiceType ChannelType
        {
            get { return _channelType; }
        }

        /// <summary>
        /// 通道是否已与工作连接关联
        /// </summary>
        private bool _isJoin = false;
        public bool IsJoin
        {
            get { return this._isJoin; }
        }

        /// <summary>
        /// 通道上下文标识
        /// </summary>
        private long _hasId;
        public long Id
        {
            get { return _hasId; }
        }

        /// <summary>
        /// 代理协议的远程会话上下文标识
        /// </summary>
        private long _remoteSessionContextId;
        public long RemoteId
        {
            get { return _remoteSessionContextId; }
            set { _remoteSessionContextId = value; }
        }

        /// <summary>
        /// 连接后保留的ACK数据包，用于与主控端连接
        /// </summary>
        byte[] _ack_buffer;
        public byte[] AckPacketData
        {
            get { return _ack_buffer; }
        }

        public event TcpChannelContextNotifyHandler<TcpSessionChannelContext, TcpChannelContextServiceType> OnChannelTypeCheckEventHandler;

        public event TcpChannelContextNotifyHandler<TcpSessionChannelContext, byte[]> OnManagerChannelMessage;

        public event TcpChannelContextNotifyHandler<TcpSessionChannelContext, byte[]> OnMainChannelMessage;

        public TcpSessionChannelContext(TcpSocketSaeaSession session)
        {
            _session = session;

            //与连接绑定，当消息接收后，会通过OnMessage接收
            session.AppTokens = new object[]
            {
                this,
                TcpChannelContextServiceType.None
            };

            //获取通道对象内存地址作为id
            //GCHandle gc = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            this._hasId = this.GetHashCode();//GCHandle.ToIntPtr(gc).ToInt64();
        }

        /// <summary>
        /// 向通道发送消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="lenght"></param>
        public void SendMessage(byte[] data, int offset, int lenght)
        {
            if (this._session == null)
                return;

            _session.SendAsync(data, offset, lenght);
        }

        /// <summary>
        /// 向关联同发送消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="lenght"></param>
        public void SendMessageToJoinSession(byte[] data, int offset, int lenght)
        {
            if (this._joinSession == null)
                return;

            _joinSession.SendAsync(data, offset, lenght);
        }

        /// <summary>
        /// 关联通道
        /// </summary>
        /// <param name="session"></param>
        /// <param name="buffer"></param>
        public void WorkChannelJoinContext(TcpSocketSaeaSession session, byte[] buffer)
        {
            _joinSession = session;

            session.AppTokens[0] = this;

            //转发给工作连接
            if (buffer.Length > 0)
                this._session.SendAsync(buffer);
            if (this._buffer.Count > 0)
            {
                this._joinSession.SendAsync(this._buffer.ToArray());
                this._buffer.Clear();
            }

            this._isJoin = true;
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="session"></param>
        public void OnMessage(TcpSocketSaeaSession session)
        {
            byte[] data = new byte[session.ReceiveBytesTransferred];
            Array.Copy(session.CompletedBuffer, 0, data, 0, data.Length);

            var type = session.AppTokens[SysContact.INDEX_CHANNELTYPE].ConvertTo<TcpChannelContextServiceType>();
            if (type == TcpChannelContextServiceType.None)
            {
                //当通道类型未确认时，进入该区域处理消息
                _buffer.AddRange(data);
                if (_buffer.Count < 4)
                    return;

                byte[] lenBytes = _buffer.GetRange(0, 4).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                if (packageLen < 0 || packageLen > 1024 * 1024 * 2)
                {
                    session.Close(true);
                    return;
                }

                if (packageLen > _buffer.Count - 4)
                    return;

                //保留ack，与控制端连接时要用到
                this._ack_buffer = _buffer.GetRange(0, packageLen + 4).ToArray();

                byte[] Ack = GZipHelper.Decompress(_buffer.GetRange(4, packageLen).ToArray());

                short headMsg = BitConverter.ToInt16(Ack, 0);

                if (headMsg == AckPacket)
                {
                    //                                                    命令头            消息体             连接类型
                    this._channelType = (TcpChannelContextServiceType)Ack[sizeof(Int16) + sizeof(Int64) + sizeof(Byte) - 1];

                    if (this._channelType == TcpChannelContextServiceType.ManagerChannel)
                    {
                        long accessKey = BitConverter.ToInt64(Ack, 2);

                        if (!AccessKeyExamine.CheckOut(accessKey))//连接密码确认
                        {
                            LogShowQueueHelper.WriteLog("ManagerChannel AccessKey Wrong " + accessKey.ToString(), "err");

                            byte[] body = MessageHelper.CommandCopyTo(MsgCommand.Msg_AccessKeyWrong);

                            byte[] pack = new byte[body.Length + sizeof(Int32)];
                            BitConverter.GetBytes(body.Length).CopyTo(pack, 0);
                            body.CopyTo(pack, 4);

                            session.SendAsync(pack);
                            //session.Close(true);
                            return;
                        }
                    }

                    this._session.AppTokens[SysContact.INDEX_CHANNELTYPE] = this._channelType;

                    if (this._channelType == TcpChannelContextServiceType.ManagerWorkChannel
                        || this._channelType == TcpChannelContextServiceType.ManagerChannel)
                    {
                        _buffer.RemoveRange(0, packageLen + 4);
                    }

                    this.OnChannelTypeCheckEventHandler?.Invoke(this, this._channelType);

                    if (this._channelType == TcpChannelContextServiceType.ManagerChannel)
                        this.ManagerPackageProcess();//如果缓冲区还有数据
                }
                else
                {
                    session.Close(true);
                }
            }
            else if (type == TcpChannelContextServiceType.WorkChannel && this._isJoin)
            {
                if (this._joinSession == null)
                    return;

                this._joinSession.SendAsync(data);
            }
            else if (type == TcpChannelContextServiceType.ManagerWorkChannel && this._channelType != type)//当前连接类型为ManagerWorkChannel且当前通道类型为ManagerWorkChannel时表示通道此时未与工作类型关联，暂时将数据存放在缓存区，待关联时再将数据转发
            {
                if (this._session == null)
                    return;

                this._session.SendAsync(data);
            }
            else if (type == TcpChannelContextServiceType.ManagerChannel)
            {
                this._buffer.AddRange(data);
                this.ManagerPackageProcess();
            }
            else if (type == TcpChannelContextServiceType.MainChannel)
            {
                this.OnMainChannelMessage?.Invoke(this, data);//直接转发
            }
            else
            {
                _buffer.AddRange(data);
            }

        }

        private void ManagerPackageProcess()
        {
            do
            {
                if (_buffer.Count < 4)
                    return;

                byte[] lenBytes = _buffer.GetRange(0, 4).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                if (packageLen > _buffer.Count - 4)
                    return;

                byte[] completedBytes = _buffer.GetRange(4, packageLen).ToArray();

                this.OnManagerChannelMessage?.Invoke(this, completedBytes);

                _buffer.RemoveRange(0, packageLen + 4);

            } while (_buffer.Count > 4);
        }
        public void OnClosed()
        {
            this._session?.Close(true);
            this._joinSession?.Close(true);
        }
    }
}
