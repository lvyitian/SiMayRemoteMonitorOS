using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SiMay.ServiceCore
{
    public class UserTrunkContext
    {
        public static UserTrunkContext UserTrunkContextInstance;

        public event Action<SessionItem[]> OnSessionItemEventHandler;

        public int CurrentSessionId
        {
            get
            {
                return _sessionId;
            }
            set
            {
                _sessionId = value;
            }
        }

        private bool _isRun = true;
        private int _port;
        private int _sessionId;
        private TcpSocketSaeaSession _trunkTcpSession;
        private TcpSocketSaeaClientAgent _socketSaeaClientAgent;
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
            if (paramText.IsNullOrEmpty())
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
                (notity, session) =>
            {
                switch (notity)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        LogHelper.DebugWriteLog("InitConntectTrunkService:OnClosed");
                        _trunkTcpSession = session;
                        SendActiveFlag();
                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceived:
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        LogHelper.DebugWriteLog("InitConntectTrunkService:OnClosed");
                        _trunkTcpSession = null;
                        SessionCloseHandler();
                        break;
                    default:
                        break;
                }
            });
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
            var sessionItems = session.CompletedBuffer.GetMessageEntity<SessionStatusPack>();
            this.OnSessionItemEventHandler?.Invoke(sessionItems.Sessions);
        }

        public void GetSessionItems()
        {
            var data = MessageHelper.CopyMessageHeadTo(TrunkMessageHead.S_EnumerateSessions);
            Send(data);
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
            _socketSaeaClientAgent.ConnectToServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port));
        }
    }
}
