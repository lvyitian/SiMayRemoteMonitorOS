using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    public static class CreateDispatcherExtension
    {
        public static TcpSessionMainConnection CreateMainServiceChannelDispatcher(this ApportionDispatcher apportionDispatcher, IDictionary<long, TcpSessionChannelDispatcher> dispatchers)
        {
            var mainServiceChannelDispatcher = new TcpSessionMainConnection(dispatchers);
            mainServiceChannelDispatcher.ACKPacketData = apportionDispatcher.GetACKPacketData();
            mainServiceChannelDispatcher.SetSession(apportionDispatcher.CurrentSession);
            var bufferData = apportionDispatcher.ListByteBuffer.ToArray();
            if (bufferData.Length > 0)//如缓冲区有数据，则处理消息
            {
                mainServiceChannelDispatcher.ListByteBuffer.AddRange(bufferData);
                mainServiceChannelDispatcher.OnMessage();
            }
            return mainServiceChannelDispatcher;
        }

        public static TcpSessionMainApplicationConnection CreateMainApplicationChannelDispatcher(this ApportionDispatcher apportionDispatcher, IDictionary<long, TcpSessionChannelDispatcher> dispatchers)
        {
            var accessId = apportionDispatcher.GetAccessId();
            var mainappChannelDispatcher = new TcpSessionMainApplicationConnection(dispatchers);
            mainappChannelDispatcher.DispatcherId = accessId;
            mainappChannelDispatcher.SetSession(apportionDispatcher.CurrentSession);

            var bufferData = apportionDispatcher.ListByteBuffer.ToArray();
            if (bufferData.Length > 0)//如缓冲区有数据，则处理消息
            {
                mainappChannelDispatcher.ListByteBuffer.AddRange(bufferData);
                mainappChannelDispatcher.OnMessage();
            }
            return mainappChannelDispatcher;
        }

        public static TcpSessionApplicationWorkerConnection CreateApplicationWorkerChannelDispatcher(this ApportionDispatcher apportionDispatcher, IDictionary<long, TcpSessionChannelDispatcher> dispatchers, ConnectionWorkType workType)
        {
            var workerConnection = new TcpSessionApplicationWorkerConnection();
            workerConnection.ConnectionWorkType = workType;
            workerConnection.SetSession(apportionDispatcher.CurrentSession);

            var bufferData = apportionDispatcher.ListByteBuffer.ToArray();
            if (bufferData.Length > 0)//如缓冲区有数据，则处理消息
                workerConnection.ListByteBuffer.AddRange(bufferData);
            return workerConnection;
        }
    }
}
