using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Awaitable;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Pooling;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;

namespace SiMay.Sockets.Tcp.Session
{
    public class TcpSocketSaeaPackBased : TcpSocketSaeaSession
    {
        internal DateTime _heartTime { get; private set; } = DateTime.Now;
        private readonly byte[] _emptyHeart = new byte[] { 0, 0, 0, 0 };
        private readonly object _opsLock = new object();
        private int _packageRecvOffset;
        private bool _isCompress;
        internal int _intervalIsUseChannel = 0;
        private byte[] _headBuffer = new byte[4];

        internal TcpSocketSaeaPackBased(
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> notifyEventHandler,
            TcpSocketSaeaEngineBased agent)
            : base(notifyEventHandler, configuration, handlerSaeaPool, sessionPool, agent)
        {
            _isCompress = configuration.CompressTransferFromPacket;
        }

        internal override void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            lock (_opsLock)
            {
                this._heartTime = DateTime.Now;
                this.State = TcpSocketConnectionState.Connected;
                this.Socket = socket;
                this.StartTime = DateTime.Now;
                this.SetSocketOptions();
            }
        }

        internal override void Detach()
        {
            lock (_opsLock)
            {
                this.Socket = null;
                this.State = TcpSocketConnectionState.None;
                this.AppTokens = null;
                this._packageRecvOffset = 0;
            }
        }

        private void SetSocketOptions()
        {
            Socket.ReceiveBufferSize = Configuration.ReceiveBufferSize;
            Socket.SendBufferSize = Configuration.SendBufferSize;
            Socket.ReceiveTimeout = (int)Configuration.ReceiveTimeout.TotalMilliseconds;
            Socket.SendTimeout = (int)Configuration.SendTimeout.TotalMilliseconds;
            Socket.NoDelay = Configuration.NoDelay;

            if (Configuration.KeepAlive)
                SetKeepAlive(Socket, 1, Configuration.KeepAliveInterval, Configuration.KeepAliveSpanTime);

            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Configuration.ReuseAddress);
        }

        private void SetKeepAlive(Socket sock, byte op, int interval, int spantime)
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes(op).CopyTo(inOptionValues, 0);//开启keepalive
            BitConverter.GetBytes((uint)interval).CopyTo(inOptionValues, Marshal.SizeOf(dummy));//多长时间开始第一次探测
            BitConverter.GetBytes((uint)spantime).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);//探测时间间隔
            sock.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        internal override void StartProcess()
        {
            var awaiter = HandlerSaeaPool.Take();

            awaiter.Saea.SetBuffer(_headBuffer, _packageRecvOffset, _headBuffer.Length);
            SaeaExHelper.ReceiveAsync(Socket, awaiter, HeadProcess);
        }

        private void HeadProcess(SaeaAwaiter awaiter, SocketError error)
        {
            if (awaiter.Saea.BytesTransferred == 0 ||
                error != SocketError.Success ||
                State != TcpSocketConnectionState.Connected ||
                Socket == null)
            {
                LogHelper.WriteLog("session_recv endtransfer state：" + State.ToString() + " socket_error：" + error.ToString());
                this.EndTransfer(awaiter);
                return;
            }

            _heartTime = DateTime.Now;
            _packageRecvOffset += awaiter.Saea.BytesTransferred;
            if (_packageRecvOffset >= 4)
            {
                int packBytesTransferred = BitConverter.ToInt32(_headBuffer, 0);

                if (packBytesTransferred < 0 || packBytesTransferred > Configuration.SendBufferSize)//长度包越界判断
                {
                    this.Close(true);
                    return;
                }

                if (packBytesTransferred == 0)
                {
                    if (this.Configuration._intervalWhetherService) //如果是服务端，则反馈心跳包
                        if (this._intervalIsUseChannel == 0)
                        {
                            var h_saea = HandlerSaeaPool.Take();

                            //4个字节空包头
                            h_saea.Saea.SetBuffer(_emptyHeart, 0, _emptyHeart.Length);
                            SaeaExHelper.SendAsync(this.Socket, h_saea, (a, e) => HandlerSaeaPool.Return(a));
                        }

                    //继续接收包头
                    _packageRecvOffset = 0;
                    awaiter.Saea.SetBuffer(_packageRecvOffset, _headBuffer.Length);
                    SaeaExHelper.ReceiveAsync(this.Socket, awaiter, HeadProcess);
                    return;
                }
                CompletedBuffer = new byte[packBytesTransferred];
                _packageRecvOffset = 0;

                awaiter.Saea.SetBuffer(CompletedBuffer, _packageRecvOffset, CompletedBuffer.Length);
                SaeaExHelper.ReceiveAsync(this.Socket, awaiter, PacketPartProcess);
            }
            else
            {
                awaiter.Saea.SetBuffer(_packageRecvOffset, _headBuffer.Length - _packageRecvOffset);
                SaeaExHelper.ReceiveAsync(this.Socket, awaiter, HeadProcess);
            }
        }
        private void PacketPartProcess(SaeaAwaiter awaiter, SocketError error)
        {
            int bytesTransferred = awaiter.Saea.BytesTransferred;
            if (bytesTransferred == 0 ||
                error != SocketError.Success ||
                State != TcpSocketConnectionState.Connected ||
                Socket == null)
            {
                LogHelper.WriteLog("session_recv endtransfer state：" + State.ToString() + " socket_error：" + error.ToString());
                this.EndTransfer(awaiter);
                return;
            }

            this.ReceiveBytesTransferred = bytesTransferred;
            this.NotifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnDataReceiveing, this);

            _heartTime = DateTime.Now;
            _packageRecvOffset += bytesTransferred;
            if (_packageRecvOffset >= CompletedBuffer.Length)
            {
                this.PackageProcess();

                _packageRecvOffset = 0;
                awaiter.Saea.SetBuffer(_headBuffer, _packageRecvOffset, _headBuffer.Length);
                SaeaExHelper.ReceiveAsync(this.Socket, awaiter, HeadProcess);
            }
            else
            {
                awaiter.Saea.SetBuffer(_packageRecvOffset, CompletedBuffer.Length - _packageRecvOffset);
                SaeaExHelper.ReceiveAsync(this.Socket, awaiter, PacketPartProcess);
            }
        }

        private void PackageProcess()
        {
            if (this._isCompress)
                CompletedBuffer = DeCompressHelper.GzipDecompress(CompletedBuffer);

            this.NotifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnDataReceived, this);
        }

        private void EndTransfer(SaeaAwaiter awaiter)
        {
            this.Close(true);
            this.HandlerSaeaPool.Return(awaiter);
            this.SessionPools.Return(this);
        }

        public override void SendAsync(byte[] data) => this.SendAsync(data, 0, data.Length);

        public override void SendAsync(byte[] data, int offset, int lenght)
        {
            if (this.Socket == null)
                return;

            byte[] buffer = this._isCompress 
                ? this.BuilderPack(DeCompressHelper.GZipCompress(data, offset, lenght)) 
                : this.BuilderPack(data, offset, lenght);

            var awaiter = HandlerSaeaPool.Take();
            awaiter.Saea.SetBuffer(buffer, 0, buffer.Length);

            Interlocked.Increment(ref _intervalIsUseChannel);
            SaeaExHelper.SendAsync(this.Socket, awaiter, (a, e) =>
             {
                 Interlocked.Decrement(ref _intervalIsUseChannel);

                 this.HandlerSaeaPool.Return(awaiter);
                 this.SendTransferredBytes = a.Saea.Buffer.Length;
                 this.NotifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnSend, this);
             });

        }

        private byte[] BuilderPack(byte[] data) 
            => this.BuilderPack(data, 0, data.Length);

        private byte[] BuilderPack(byte[] data, int offset, int lenght)
        {
            byte[] buffer = new byte[data.Length + sizeof(Int32)];
            var bodyLen = BitConverter.GetBytes(data.Length);
            Array.Copy(bodyLen, 0, buffer, 0, bodyLen.Length);
            Array.Copy(data, offset, buffer, 4, lenght);

            return buffer;
        }

        public override void Close(bool notify)
        {
            lock (_opsLock)
            {
                if (Socket != null)
                {
                    try
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                        Socket.Close();
                    }
                    catch (Exception e)
                    {
                        LogHelper.WriteLog("session--close info：" + e.Message);
                    }
                    finally
                    {
                        Socket = null;
                    }
                    State = TcpSocketConnectionState.Closed;

                    if (notify)
                    {
                        this.NotifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnClosed, this);
                    }
                }
            }
        }
    }
}
