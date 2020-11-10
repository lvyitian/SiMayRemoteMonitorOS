using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;

namespace SiMay.Net.SessionProvider
{
    public class TcpProxyMainConnectionContext
    {
        /// <summary>
        /// 代理会话通知
        /// </summary>
        public event Action<TcpProxyApplicationConnectionContext, TcpSessionNotify> SessionNotifyEventHandler;

        /// <summary>
        /// 登出
        /// </summary>
        public event Action<TcpProxyMainConnectionContext, string> LogOutEventHandler;

        /// <summary>
        /// Id或者Key错误
        /// </summary>
        public event Action<TcpProxyMainConnectionContext> AccessIdOrKeyWrongEventHandler;

        /// <summary>
        /// 发起一个应用连接
        /// </summary>
        public event Action<TcpProxyMainConnectionContext> LaunchApplicationConnectEventHandler;

        private TcpSocketSaeaSession _currentSession;
        private IDictionary<long, SessionProviderContext> _proxySessions = new Dictionary<long, SessionProviderContext>();

        public TcpProxyMainConnectionContext(TcpSocketSaeaSession session) => _currentSession = session;

        public void OnProcess(byte[] data)
        {
            switch (data.GetMessageHead<MessageHead>())
            {
                case MessageHead.MID_SESSION:
                    this.HandleOnCreateSession(data);
                    break;
                case MessageHead.MID_SESSION_CLOSED:
                    this.HandleOnClosed(data);
                    break;
                case MessageHead.MID_APPWORK:
                    this.LaunchApplicationConnectEventHandler?.Invoke(this);
                    break;
                case MessageHead.MID_MESSAGE_DATA:
                    this.HandleOnMessage(data);
                    break;
                case MessageHead.MID_ACCESS_KEY_WRONG:
                    this.AccessIdOrKeyWrongEventHandler?.Invoke(this);
                    break;
                case MessageHead.MID_LOGOUT:
                    this.HandleOnLogOut(data);
                    break;
                default:
                    break;
            }
        }

        private void HandleOnCreateSession(byte[] data)
        {
            var sessions = data.GetMessageEntity<SessionPacket>();
            foreach (var session in sessions.SessionItems)
            {
                if (!this._proxySessions.ContainsKey(session.Id))
                {
                    var proxyConnectionContext = new TcpProxyApplicationConnectionContext();
                    proxyConnectionContext.DataReceivedEventHandler += HandleOnDataReceived;
                    proxyConnectionContext.DataSendEventHandler += HandleOnDataSend;
                    proxyConnectionContext.SetSession(_currentSession, session.Id, session.ACKPacketData);
                    this._proxySessions.Add(session.Id, proxyConnectionContext);
                    this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnConnected);
                    this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnDataReceived);
                }
            }
        }

        private void HandleOnDataSend(TcpProxyApplicationConnectionContext proxyConnectionContext)
            => this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnSend);

        private void HandleOnDataReceived(TcpProxyApplicationConnectionContext proxyConnectionContext)
            => this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnDataReceived);

        private void HandleOnClosed(byte[] data)
        {
            var closedPack = data.GetMessageEntity<SessionClosedPacket>();
            if (_proxySessions.ContainsKey(closedPack.Id))
            {
                var proxyConnectionContext = _proxySessions.GetValue(closedPack.Id).ConvertTo<TcpProxyApplicationConnectionContext>();
                proxyConnectionContext.DataReceivedEventHandler -= HandleOnDataReceived;
                proxyConnectionContext.DataSendEventHandler -= HandleOnDataSend;
                this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnClosed);
                _proxySessions.Remove(closedPack.Id);
                proxyConnectionContext.Dispose();
            }
        }

        private void HandleOnMessage(byte[] data)
        {
            var message = data.GetMessageEntity<MessageDataPacket>();
            if (_proxySessions.ContainsKey(message.DispatcherId))
            {
                var proxyConnectionContext = _proxySessions.GetValue(message.DispatcherId).ConvertTo<TcpProxyApplicationConnectionContext>();
                proxyConnectionContext.ListByteBuffer.AddRange(message.Data);
                proxyConnectionContext.OnMessage(message.Data.Length);
                this.SessionNotifyEventHandler?.Invoke(proxyConnectionContext, TcpSessionNotify.OnDataReceiveing);
            }
        }

        private void HandleOnLogOut(byte[] data)
        {
            var logOut = data.GetMessageEntity<LogOutPacket>();
            this.LogOutEventHandler?.Invoke(this, logOut.Message);
        }

        /// <summary>
        /// 获取Session
        /// </summary>
        public void PulCurrentSessions()
        {
            var data = MessageHelper.CopyMessageHeadTo(MessageHead.APP_PULL_SESSION);
            _currentSession.SendAsync(data);
        }

        public void CloseCurrentSession()
        {
            foreach (TcpProxyApplicationConnectionContext proxyContext in _proxySessions.Select(c => c.Value))
            {
                this.SessionNotifyEventHandler?.Invoke(proxyContext, TcpSessionNotify.OnClosed);
                proxyContext.DataReceivedEventHandler -= HandleOnDataReceived;
                proxyContext.DataSendEventHandler -= HandleOnDataSend;
                proxyContext.Dispose();
            }
            _currentSession.Close(true);
        }
    }
}
