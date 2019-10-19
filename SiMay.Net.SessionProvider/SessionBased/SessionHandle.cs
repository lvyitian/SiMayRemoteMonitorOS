using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Net.SessionProvider.SessionBased
{
    public abstract class SessionHandler
    {
        public Socket Socket { get; set; }
        public object[] AppTokens { get; set; }
        public abstract int SendTransferredBytes { get;}
        public abstract int ReceiveTransferredBytes { get;}
        public abstract byte[] CompletedBuffer { get; }
        public abstract int Send(byte[] data);
        public abstract int Send(byte[] data, int offset, int length);
        public abstract void SendAsync(byte[] data);
        public abstract void SendAsync(byte[] data, int offset, int length);
        public abstract void SessionClose();
    }
}