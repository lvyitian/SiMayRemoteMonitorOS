using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider.SessionBased
{
    public class TcpSocketSessionBased : SessionProviderContext
    {
        TcpSocketSaeaSession _session;
        internal TcpSocketSessionBased(TcpSocketSaeaSession session)
        {
            Socket = session.Socket;
            _session = session;
        }

        public override int SendTransferredBytes
        {
            get
            {
                return _session.SendTransferredBytes;
            }
        }
        public override int ReceiveTransferredBytes
        {
            get
            {
                return _session.ReceiveBytesTransferred;
            }
        }
        public override byte[] CompletedBuffer
        {
            get
            {
                return _session.CompletedBuffer;
            }
        }

        public override void SendAsync(byte[] data) 
            => _session.SendAsync(data);

        public override void SendAsync(byte[] data, int offset, int length) 
            => _session.SendAsync(data, offset, length);

        public override void SessionClose() 
            => _session.Close(true);
    }
}
