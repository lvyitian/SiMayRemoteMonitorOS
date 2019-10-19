using System;
using System.Net.Sockets;
using System.Threading;

namespace SiMay.Sockets.Tcp.Awaitable
{
    public class SaeaExHelper
    {
        private static readonly Func<Socket, SaeaAwaiter, bool> ACCEPT = (s, a) => s.AcceptAsync(a.Saea);
        private static readonly Func<Socket, SaeaAwaiter, bool> CONNECT = (s, a) => s.ConnectAsync(a.Saea);
        private static readonly Func<Socket, SaeaAwaiter, bool> DISCONNECT = (s, a) => s.DisconnectAsync(a.Saea);
        private static readonly Func<Socket, SaeaAwaiter, bool> RECEIVE = (s, a) => s.ReceiveAsync(a.Saea);
        private static readonly Func<Socket, SaeaAwaiter, bool> SEND = (s, a) => s.SendAsync(a.Saea);

        public static void AcceptAsync(Socket socket, SaeaAwaiter awaiter, Action<SaeaAwaiter, SocketError> callback)
            => OperateAsync(socket, awaiter, ACCEPT, callback);
        public static void ConnectAsync(Socket socket, SaeaAwaiter awaiter, Action<SaeaAwaiter, SocketError> callback)
            => OperateAsync(socket, awaiter, CONNECT, callback);

        public static void DisonnectAsync(Socket socket, SaeaAwaiter awaiter, Action<SaeaAwaiter, SocketError> callback)
            => OperateAsync(socket, awaiter, DISCONNECT, callback);

        public static void ReceiveAsync(Socket socket, SaeaAwaiter awaiter, Action<SaeaAwaiter, SocketError> callback)
            => OperateAsync(socket, awaiter, RECEIVE, callback);

        public static void SendAsync(Socket socket, SaeaAwaiter awaiter, Action<SaeaAwaiter, SocketError> callback)
            => OperateAsync(socket, awaiter, SEND, callback);

        private static void OperateAsync(Socket socket, SaeaAwaiter awaiter, Func<Socket, SaeaAwaiter, bool> operation, Action<SaeaAwaiter, SocketError> callback)
        {

            if (socket == null) return;

            try
            {
                awaiter.Reset();
                awaiter.OnCompleted(callback);
                operation.Invoke(socket, awaiter);
            }
            catch (Exception e)
            {
                Console.WriteLine("simaysocket operate exception:" + e.Message);
            }

        }
    }
}
