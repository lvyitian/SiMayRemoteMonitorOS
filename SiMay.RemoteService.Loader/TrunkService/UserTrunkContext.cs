using SiMay.ModelBinder;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SiMay.RemoteService.Loader
{
    public class UserTrunkContext
    {
        private const string LOOP_BACK_ADDRESS = "127.0.0.1";
        public static UserTrunkContext UserTrunkContextInstance { get; set; }

        public int CurrentSessionId
        {
            get
            {
                return _sessionId;
            }
        }

        private bool _isRun = true;
        private int _port;
        private int _sessionId;
        private TcpSocketSaeaSession _trunkTcpSession;
        private TcpSocketSaeaClientAgent _socketSaeaClientAgent;
        private AutoResetEvent _autoReset = new AutoResetEvent(false);
        private PacketModelBinder<TcpSocketSaeaSession, TrunkMessageHead> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession, TrunkMessageHead>();
        public UserTrunkContext(string[] args)
        {
            if (!int.TryParse(GetValueParse(args, "-port"), out _port))
                return;
            if (!int.TryParse(GetValueParse(args, "-sessionId"), out _sessionId))
                return;

            InitConntectTrunkService();
            LaunchConnectTrunkService();
            UserTrunkContext.UserTrunkContextInstance = this;
        }

        private string GetValueParse(string[] args, string name)
        {
            var paramText = args.FirstOrDefault(c => c.Contains(name));
            if (string.IsNullOrEmpty(paramText))
                return null;
            var paramSplits = paramText.Split(':');
            if (paramSplits.Length > 1)
                return paramSplits[1];
            else
                return null;
        }

        private void InitConntectTrunkService()
        {
            var clientConfig = new TcpSocketSaeaClientConfiguration();
            _socketSaeaClientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfig,
                (Sockets.Delegate.NotifyEventHandler<TcpSessionNotify, TcpSocketSaeaSession>)((notity, session) =>
            {
                switch (notity)
                {
                    case TcpSessionNotify.OnConnected:
                        _trunkTcpSession = session;
                        SendActiveFlag();
                        break;
                    case TcpSessionNotify.OnSend:
                        break;
                    case TcpSessionNotify.OnDataReceiveing:
                        break;
                    case TcpSessionNotify.OnDataReceived:
                        _handlerBinder.CallFunctionPacketHandler((TcpSocketSaeaSession)session, (TrunkMessageHead)session.CompletedBuffer.GetMessageHead<TrunkMessageHead>(), (object)this);
                        break;
                    case TcpSessionNotify.OnClosed:
                        _trunkTcpSession = null;
                        _autoReset.Set();
                        SessionCloseHandler();
                        break;
                    default:
                        break;
                }
            }));
        }

        private void SessionCloseHandler()
        {
            if (!_isRun)
                return;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += (s, e) =>
            {
                LaunchConnectTrunkService();

                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        [PacketHandler(TrunkMessageHead.C_SessionItems)]
        private void SessionItemsHandler(TcpSocketSaeaSession session)
        {
            Thread.Sleep(100);//延迟一会，防止WaitOne之前Set
            _autoReset.Set();
        }

        public SessionItem[] GetSessionItems()
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_EnumerateSessions);
            Send(data);
            _autoReset.WaitOne();
            if (_trunkTcpSession == null)
                return new SessionItem[0];

            var sessionItems = _trunkTcpSession.CompletedBuffer.GetMessageEntity<SessionStatusPack>();
            return sessionItems.Sessions;
        }

        public void CreateProcessAsUser(int sessionId)
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_CreateUserProcess,
                new CreateUserProcessPack()
                {
                    SessionId = sessionId
                });
            Send(data);
        }

        public void SendSas()
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_SendSas);
            Send(data);
        }

        public void InitiativeExit()
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_InitiativeExit);
            Send(data);
            _isRun = false;
        }
        private void SendActiveFlag()
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_Active, new ActivePack()
            {
                SessionId = _sessionId
            });
            Send(data);
        }

        private void Send(byte[] data)
        {
            if (_trunkTcpSession == null)
                return;
            _trunkTcpSession.SendAsync(data);
        }
        private void LaunchConnectTrunkService()
        {
            _socketSaeaClientAgent.ConnectToServer(new IPEndPoint(IPAddress.Parse(LOOP_BACK_ADDRESS), _port));
        }
    }
}
