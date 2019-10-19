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
    public class TcpChannelContext
    {
        //缓冲区同步
        readonly object _lock = new object();
        const short AckPacket = 1000;

        bool _isJoin = false;
        List<byte> _buffer = new List<byte>();
        TcpSocketSaeaSession _desession;
        TcpSocketSaeaSession _session;
        TcpChannelContextServiceType _channelType = TcpChannelContextServiceType.None;

        public TcpSocketSaeaSession Session
        {
            get { return _session; }
        }
        public byte[] GetBuffer
        {
            get { return this._buffer.ToArray(); }
        }
        public TcpChannelContextServiceType ChannelType
        {
            get { return _channelType; }
        }

        public bool IsJoin
        {
            get { return this._isJoin; }
        }

        long _id;
        public long Id
        {
            get { return _id; }
        }

        long _rid;
        public long RemoteId
        {
            get { return _rid; }
            set { _rid = value; }
        }

        byte[] _ack_buffer;
        public byte[] AckPack
        {
            get { return _ack_buffer; }
        }

        public event TcpChannelContextNotifyHandler<TcpChannelContext, TcpChannelContextServiceType> OnChannelTypeNotify;

        public event TcpChannelContextNotifyHandler<TcpChannelContext, byte[]> OnManagerChannelMessage;

        public event TcpChannelContextNotifyHandler<TcpChannelContext, byte[]> OnMainChannelMessage;

        public TcpChannelContext(TcpSocketSaeaSession session)
        {
            _session = session;

            //与连接绑定，当消息接收后，会通过OnMessage接收
            session.AppTokens = new object[]
            {
                this,
                TcpChannelContextServiceType.None
            };

            //获取通道对象内存地址作为id
            GCHandle gc = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            this._id = GCHandle.ToIntPtr(gc).ToInt64();
        }

        public void SendMessage(byte[] data, int offset, int lenght)
        {
            if (this._session == null)
                return;

            _session.SendAsync(data, offset, lenght);
        }

        public void WorkChannelJoinContext(TcpSocketSaeaSession session, byte[] buffer)
        {
            _desession = session;

            session.AppTokens[0] = this;

            //转发给工作连接
            if (buffer.Length > 0)
                this._session.SendAsync(buffer);

            if (this._buffer.Count > 0)
            {
                this._desession.SendAsync(this._buffer.ToArray());
                this._buffer.Clear();
            }

            this._isJoin = true;
        }

        public void OnMessage(TcpSocketSaeaSession session)
        {
            byte[] data = new byte[session.ReceiveBytesTransferred];
            Array.Copy(session.CompletedBuffer, 0, data, 0, data.Length);

            var type = (TcpChannelContextServiceType)session.AppTokens[1];
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

                byte[] Ack = CompressHelper.Decompress(_buffer.GetRange(4, packageLen).ToArray());

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

                    this._session.AppTokens[1] = this._channelType;

                    if (this._channelType == TcpChannelContextServiceType.ManagerWorkChannel
                        || this._channelType == TcpChannelContextServiceType.ManagerChannel)
                    {
                        _buffer.RemoveRange(0, packageLen + 4);
                    }

                    this.OnChannelTypeNotify?.Invoke(this, this._channelType);

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
                if (this._desession == null)
                    return;

                this._desession.SendAsync(data);
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
            this._desession?.Close(true);
            this.DisposeContext();
        }

        public void DisposeContext()
        {
            //this._buffer.Clear();
            this._desession = null;
            this._session = null;

            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(this.Id));
            gc.Free();
        }
    }
}
