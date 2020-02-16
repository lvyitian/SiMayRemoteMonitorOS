using System;
using System.Collections.Generic;
using System.Text;
using SiMay.Net.SessionProvider.Core;

namespace SiMay.Net.SessionProviderServiceCore
{
    public class TcpSessionApplicationWorkerConnection : TcpSessionChannelDispatcher
    {
        /// <summary>
        /// 是否已与工作连接关联
        /// </summary>
        public bool IsJoin { get; private set; }

        private TcpSessionApplicationWorkerConnection _targetConnection;

        public void Join(TcpSessionApplicationWorkerConnection targetConnection)
        {
            if (IsJoin)
                return;
            IsJoin = true;
            _targetConnection = targetConnection;
            targetConnection.Join(this);
        }
        public override void OnMessage()
        {
            base.OnMessage();
            if (ListByteBuffer.Count > 0)
            {
                this._targetConnection.SendTo(ListByteBuffer.ToArray());
                ListByteBuffer.Clear();
            }
        }
        public override void OnClosed()
        {
            if (_targetConnection.CurrentSession.State == Sockets.Tcp.TcpSocketConnectionState.Closed)
                return;
            _targetConnection.CloseSession();
            ListByteBuffer.Clear();
        }
    }
}
