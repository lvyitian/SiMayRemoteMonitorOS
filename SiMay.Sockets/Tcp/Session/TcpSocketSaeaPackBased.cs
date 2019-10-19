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
        private readonly byte[] _emptyHeart = new byte[] { 0, 0, 0, 0 };
        private int _packageRecvOffset;
        private bool _iscompress;
        private byte[] _headbuffer = new byte[4];


        internal int _isuchannel = 0;
        internal DateTime _heartTime { get; private set; } = DateTime.Now;

        internal TcpSocketSaeaPackBased(
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> notifyEventHandler,
            LogHelper logHelper,
            TcpSocketSaeaEngineBased agent)
            : base(notifyEventHandler, configuration, handlerSaeaPool, sessionPool, agent, logHelper)
        {
            _iscompress = configuration.CompressTransferFromPacket;
        }

        internal override void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            lock (_opsLock)
            {
                this._heartTime = DateTime.Now;
                this._state = TcpSocketConnectionState.Connected;
                this._socket = socket;
                this._startTime = DateTime.UtcNow;
                this.SetSocketOptions();
            }
        }

        internal override void Detach()
        {
            lock (_opsLock)
            {
                this._socket = null;
                this._state = TcpSocketConnectionState.None;
                this.AppTokens = null;
                this._packageRecvOffset = 0;
            }
        }

        private void SetSocketOptions()
        {
            _socket.ReceiveBufferSize = _configuration.ReceiveBufferSize;
            _socket.SendBufferSize = _configuration.SendBufferSize;
            _socket.ReceiveTimeout = (int)_configuration.ReceiveTimeout.TotalMilliseconds;
            _socket.SendTimeout = (int)_configuration.SendTimeout.TotalMilliseconds;
            _socket.NoDelay = _configuration.NoDelay;

            if (_configuration.KeepAlive)
                SetKeepAlive(_socket, 1, _configuration.KeepAliveInterval, _configuration.KeepAliveSpanTime);

            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, _configuration.ReuseAddress);
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
            var awaiter = _handlerSaeaPool.Take();

            awaiter.Saea.SetBuffer(_headbuffer, _packageRecvOffset, _headbuffer.Length);
            SaeaExHelper.ReceiveAsync(_socket, awaiter, HeadProcess);
        }

        private void HeadProcess(SaeaAwaiter awaiter, SocketError error)
        {
            if (awaiter.Saea.BytesTransferred == 0 ||
                error != SocketError.Success ||
                _state != TcpSocketConnectionState.Connected ||
                _socket == null)
            {
                _logger.WriteLog("session_recv endtransfer state：" + _state.ToString() + " socket_error：" + error.ToString());
                this.EndTransfer(awaiter);
                return;
            }

            _heartTime = DateTime.Now;
            _packageRecvOffset += awaiter.Saea.BytesTransferred;
            if (_packageRecvOffset >= 4)
            {
                int packBytesTransferred = BitConverter.ToInt32(_headbuffer, 0);

                if (packBytesTransferred < 0 || packBytesTransferred > _configuration.SendBufferSize)//长度包越界判断
                {
                    this.InternalClose(true);
                    return;
                }

                if (packBytesTransferred == 0)
                {
                    if (this._configuration._SessionIsService) //如果是服务端，则反馈心跳包
                        if (this._isuchannel == 0)
                        {
                            var h_saea = _handlerSaeaPool.Take();

                            //4个字节空包头
                            h_saea.Saea.SetBuffer(_emptyHeart, 0, _emptyHeart.Length);
                            SaeaExHelper.SendAsync(this._socket, h_saea, (a, e) => _handlerSaeaPool.Return(a));
                        }

                    //继续接收包头
                    _packageRecvOffset = 0;
                    awaiter.Saea.SetBuffer(_packageRecvOffset, _headbuffer.Length);
                    SaeaExHelper.ReceiveAsync(this._socket, awaiter, HeadProcess);
                    return;
                }
                _completebuffer = new byte[packBytesTransferred];
                _packageRecvOffset = 0;

                awaiter.Saea.SetBuffer(_completebuffer, _packageRecvOffset, _completebuffer.Length);
                SaeaExHelper.ReceiveAsync(this._socket, awaiter, PacketPartProcess);
            }
            else
            {
                awaiter.Saea.SetBuffer(_packageRecvOffset, _headbuffer.Length - _packageRecvOffset);
                SaeaExHelper.ReceiveAsync(this._socket, awaiter, HeadProcess);
            }
        }
        private void PacketPartProcess(SaeaAwaiter awaiter, SocketError error)
        {
            int bytesTransferred = awaiter.Saea.BytesTransferred;
            if (bytesTransferred == 0 ||
                error != SocketError.Success ||
                _state != TcpSocketConnectionState.Connected ||
                _socket == null)
            {
                _logger.WriteLog("session_recv endtransfer state：" + _state.ToString() + " socket_error：" + error.ToString());
                this.EndTransfer(awaiter);
                return;
            }

            this.ReceiveBytesTransferred = bytesTransferred;
            this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnDataReceiveing, this);

            _heartTime = DateTime.Now;
            _packageRecvOffset += bytesTransferred;
            if (_packageRecvOffset >= _completebuffer.Length)
            {
                this.PackageProcess();

                _packageRecvOffset = 0;
                awaiter.Saea.SetBuffer(_headbuffer, _packageRecvOffset, _headbuffer.Length);
                SaeaExHelper.ReceiveAsync(this._socket, awaiter, HeadProcess);
            }
            else
            {
                awaiter.Saea.SetBuffer(_packageRecvOffset, _completebuffer.Length - _packageRecvOffset);
                SaeaExHelper.ReceiveAsync(this._socket, awaiter, PacketPartProcess);
            }
        }

        private void PackageProcess()
        {
            if (this._iscompress)
                _completebuffer = CompressHelper.Decompress(_completebuffer);

            this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnDataReceived, this);
        }

        private void EndTransfer(SaeaAwaiter awaiter)
        {
            this.InternalClose(true);
            this._handlerSaeaPool.Return(awaiter);
            this._sessionPool.Return(this);
        }

        public override int Send(byte[] data)
           => this.Send(data, 0, data.Length);

        public override int Send(byte[] data, int offset, int length)
        {
            int count = 0;
            if (this._socket == null)
                return count;

            byte[] buffer = this._iscompress
                ? this.BuilderPack(CompressHelper.Compress(data, offset, length))
                : this.BuilderPack(data, offset, length);

            Interlocked.Increment(ref _isuchannel);
            try
            {
                count = this._socket.Send(buffer);
            }
            catch { }
            finally {
                this.SendBytesTransferred = count;
                this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnSend, this);
                Interlocked.Decrement(ref _isuchannel);
            }
            return count;
        }

        public override void SendAsync(byte[] data) => this.SendAsync(data, 0, data.Length);

        public override void SendAsync(byte[] data, int offset, int length)
        {
            if (this._socket == null)
                return;

            byte[] buffer = this._iscompress 
                ? this.BuilderPack(CompressHelper.Compress(data, offset, length)) 
                : this.BuilderPack(data, offset, length);

            var awaiter = _handlerSaeaPool.Take();
            awaiter.Saea.SetBuffer(buffer, 0, buffer.Length);
            
            Interlocked.Increment(ref _isuchannel);
            SaeaExHelper.SendAsync(this._socket, awaiter, (a, e) =>
             {
                 Interlocked.Decrement(ref _isuchannel);
                 this.SendBytesTransferred = a.Saea.Buffer.Length;
                 this._handlerSaeaPool.Return(a);
                 this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnSend, this);
                 if (e != SocketError.Success)
                     _logger.WriteLog(string.Format("SendAsync--exception:{0}", e.ToString()));
             });

        }

        private byte[] BuilderPack(byte[] data) => this.BuilderPack(data, 0, data.Length);
        private byte[] BuilderPack(byte[] data, int offset, int lenght)
        {
            byte[] buffer = new byte[data.Length + sizeof(Int32)];
            var bodyLen = BitConverter.GetBytes(data.Length);
            Array.Copy(bodyLen, 0, buffer, 0, bodyLen.Length);
            Array.Copy(data, offset, buffer, 4, lenght);

            return buffer;
        }

        private void InternalClose(bool notify)
        {
            lock (_opsLock)
            {
                if (_socket != null)
                {
                    try
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Close();
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog("session--close info：" + e.Message);
                    }
                    finally
                    {
                        _socket = null;
                    }
                    _state = TcpSocketConnectionState.Closed;

                    if (notify)
                    {
                        this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnClosed, this);
                    }
                }
            }
        }
        public override void Close(bool notify)
        {
            _logger.WriteLog("initiative--close");
            this.InternalClose(notify);
        }
    }
}
