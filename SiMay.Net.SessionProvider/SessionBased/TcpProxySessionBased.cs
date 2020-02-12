using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SiMay.Net.SessionProvider.SessionBased
{
    public class TcpProxySessionBased : SessionProviderContext, IDisposable
    {
        long _id;
        internal long Id { get { return _id; } }

        internal long RemoteId { get; set; }
        internal List<byte> Buffer = new List<byte>();

        TcpSocketSaeaSession _session;
        int _disposable = 0;
        public TcpProxySessionBased(TcpSocketSaeaSession session)
        {
            Socket = session.Socket;
            _session = session;

            GCHandle gc = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            this._id = GCHandle.ToIntPtr(gc).ToInt64();
        }

        internal int _sendTransferredBytes;
        public override int SendTransferredBytes
        {
            get { return this._sendTransferredBytes; }
        }

        internal int _receiveTransferredBytes;
        public override int ReceiveTransferredBytes
        {
            get { return this._receiveTransferredBytes; }
        }

        internal byte[] _completedBuffer;
        public override byte[] CompletedBuffer
        {
            get { return _completedBuffer; }
        }

        public override void SendAsync(byte[] data)
        {
            this.SendAsync(data, 0, data.Length);
        }

        public override void SendAsync(byte[] data, int offset, int length)
        {
            if (this._disposable == 1)
                return;

            byte[] bytes = GZipHelper.Compress(data, offset, length);
            if ((SessionWorkType)_session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                byte[] body = new byte[sizeof(Int64) + sizeof(Int32) + bytes.Length];
                BitConverter.GetBytes(this.RemoteId).CopyTo(body, 0);
                BitConverter.GetBytes(bytes.Length).CopyTo(body, 8);
                bytes.CopyTo(body, 12);

                MessageHelper.SendMessage(_session, MessageHead.Msg_MessageData, body);
            }
            else
            {
                byte[] body = new byte[bytes.Length + sizeof(Int32)];
                BitConverter.GetBytes(bytes.Length).CopyTo(body, 0);
                bytes.CopyTo(body, 4);

                _session.SendAsync(body, 0, body.Length);
            }
        }

        public override void SessionClose()
        {
            if (this._disposable == 1)
                return;

            if ((SessionWorkType)_session.AppTokens[0] == SessionWorkType.ManagerSession)
            {
                MessageHelper.SendMessage(_session, MessageHead.Msg_Close_Session, BitConverter.GetBytes(this.RemoteId));
            }
            else
            {
                _session.Close(true);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposable, 1) == 0)
            {
                //this.Buffer.Clear();
                //this.AppTokens = null;

                GCHandle gc = GCHandle.FromIntPtr(new IntPtr(this.Id));
                gc.Free();
            }
        }
    }
}
