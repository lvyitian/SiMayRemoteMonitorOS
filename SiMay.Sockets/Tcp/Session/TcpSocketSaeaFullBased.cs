using SiMay.Sockets.Delegate;
using SiMay.Sockets.Tcp.Awaitable;
using SiMay.Sockets.Tcp.Pooling;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using SiMay.Sockets.UtilityHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SiMay.Sockets.Tcp.Session
{
    public class TcpSocketSaeaFullBased : TcpSocketSaeaSession
    {
        internal TcpSocketSaeaFullBased(
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            NotifyEventHandler<TcpSocketCompletionNotify, TcpSocketSaeaSession> notifyEventHandler,
            LogHelper logHelper,
            TcpSocketSaeaEngineBased agent)
            : base(notifyEventHandler, configuration, handlerSaeaPool, sessionPool, agent, logHelper) 
            => _completebuffer = new byte[configuration.ReceiveBufferSize];

        internal override void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket NULL");

            lock (_opsLock)
            {
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
            }
        }

        private void SetSocketOptions()
        {
            _socket.ReceiveBufferSize = _configuration.ReceiveBufferSize;
            _socket.SendBufferSize = _configuration.SendBufferSize;
            _socket.ReceiveTimeout = (int)_configuration.ReceiveTimeout.TotalMilliseconds;
            _socket.SendTimeout = (int)_configuration.SendTimeout.TotalMilliseconds;
            _socket.NoDelay = _configuration.NoDelay;

            if (_configuration.AppKeepAlive)
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

            awaiter.Saea.SetBuffer(_completebuffer, 0, _completebuffer.Length);
            SaeaExHelper.ReceiveAsync(_socket, awaiter, PacketPartProcess);
        }

        private void PacketPartProcess(SaeaAwaiter awaiter, SocketError error)
        {
            var bytesTransferred = awaiter.Saea.BytesTransferred;
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

            _notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnDataReceiveing, this);

            awaiter.Saea.SetBuffer(_completebuffer, 0, _completebuffer.Length);
            SaeaExHelper.ReceiveAsync(this._socket, awaiter, PacketPartProcess);
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

            try
            {
                count = this._socket.Send(data, offset, length, SocketFlags.None);
            }
            catch { }
            finally
            {
                this.SendBytesTransferred = count;
                this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnSend, this);
            }
            return count;
        }

        public override void SendAsync(byte[] data) => this.SendAsync(data, 0, data.Length);

        public override void SendAsync(byte[] data, int offset, int length)
        {
            if (this._socket == null)
                return;

            var awaiter = _handlerSaeaPool.Take();
            awaiter.Saea.SetBuffer(data, offset, length);

            SaeaExHelper.SendAsync(this._socket, awaiter, (a, e) =>
            {
                this.SendBytesTransferred = a.Saea.Buffer.Length;

                _handlerSaeaPool.Return(awaiter);
                this._notifyEventHandler?.Invoke(TcpSocketCompletionNotify.OnSend, this);
            });
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
                        _logger.WriteLog("session_close info：" + e.Message);
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
            _logger.WriteLog("initiative_close");
            this.InternalClose(notify);
        }
    }
}
