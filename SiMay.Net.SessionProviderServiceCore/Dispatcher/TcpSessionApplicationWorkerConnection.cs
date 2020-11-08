using System;
using System.Collections.Generic;
using System.Text;
using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;

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

        public override void OnBefore(byte[] data) => this._targetConnection.SendTo(data);

        public override void OnProcess()
        {
            base.OnProcess();
            if (ListByteBuffer.Count > 0)
            {
                this._targetConnection.SendTo(ListByteBuffer.ToArray());
                ListByteBuffer.Clear();
            }
        }
        public override void OnClosed()
        {
            if (_targetConnection.CurrentSession.State == TcpSocketConnectionState.Closed)
                return;
            _targetConnection.CloseSession();
            ListByteBuffer.Clear();
        }
    }
}
