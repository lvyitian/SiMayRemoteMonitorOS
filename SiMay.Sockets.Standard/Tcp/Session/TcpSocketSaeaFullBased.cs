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
        private readonly object _opsLock = new object();

        internal TcpSocketSaeaFullBased(
            TcpSocketConfigurationBase configuration,
            SaeaAwaiterPool handlerSaeaPool,
            SessionPool sessionPool,
            NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession> notifyEventHandler,
            TcpSocketSaeaEngineBased agent)
            : base(notifyEventHandler, configuration, handlerSaeaPool, sessionPool, agent) 
            => CompletedBuffer = new byte[configuration.ReceiveBufferSize];

        internal override void Attach(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket NULL");

            lock (_opsLock)
            {
                this.State = TcpSocketConnectionState.Connected;
                this.Socket = socket;
                this.StartTime = DateTime.UtcNow;
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
            }
        }

        private void SetSocketOptions()
        {
            Socket.ReceiveBufferSize = Configuration.ReceiveBufferSize;
            Socket.SendBufferSize = Configuration.SendBufferSize;
            Socket.ReceiveTimeout = (int)Configuration.ReceiveTimeout.TotalMilliseconds;
            Socket.SendTimeout = (int)Configuration.SendTimeout.TotalMilliseconds;
            Socket.NoDelay = Configuration.NoDelay;

            if (Configuration.AppKeepAlive)
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

            awaiter.Saea.SetBuffer(CompletedBuffer, 0, CompletedBuffer.Length);
            SaeaExHelper.ReceiveAsync(Socket, awaiter, PacketPartProcess);
        }

        private void PacketPartProcess(SaeaAwaiter awaiter, SocketError error)
        {
            var bytesTransferred = awaiter.Saea.BytesTransferred;
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

            NotifyEventHandler?.Invoke(TcpSessionNotify.OnDataReceiveing, this);

            awaiter.Saea.SetBuffer(CompletedBuffer, 0, CompletedBuffer.Length);
            SaeaExHelper.ReceiveAsync(this.Socket, awaiter, PacketPartProcess);
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

            var awaiter = HandlerSaeaPool.Take();
            awaiter.Saea.SetBuffer(data, offset, lenght);

            SaeaExHelper.SendAsync(this.Socket, awaiter, (a, e) =>
            {
                this.SendTransferredBytes = a.Saea.Buffer.Length;

                HandlerSaeaPool.Return(awaiter);
                this.NotifyEventHandler?.Invoke(TcpSessionNotify.OnSend, this);
            });
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
                        LogHelper.WriteLog("session_close info：" + e.Message);
                    }
                    finally
                    {
                        Socket = null;
                    }
                    State = TcpSocketConnectionState.Closed;

                    if (notify)
                    {
                        this.NotifyEventHandler?.Invoke(TcpSessionNotify.OnClosed, this);
                    }
                }
            }
        }
    }
}
