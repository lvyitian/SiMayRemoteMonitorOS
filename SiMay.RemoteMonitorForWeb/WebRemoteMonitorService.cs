using Newtonsoft.Json;
using SiMay.Basic;
using SiMay.Net.SessionProvider;
using SiMay.RemoteControlsCore;
using SuperSocket.SocketBase;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public class WebRemoteMonitorService
    {
        private MainApplicationAdapterHandler _appMainAdapterHandler = new MainApplicationAdapterHandler(new SystemAppConfig());
        private IDictionary<string, IDesktopView> _serivceDesktopViews = new Dictionary<string, IDesktopView>();
        public void LanuchService()
        {
            _appMainAdapterHandler.ViewRefreshInterval = AppConfiguration.DesktopRefreshInterval;
            _appMainAdapterHandler.OnProxyNotifyHandlerEvent += OnProxyNotify;
            _appMainAdapterHandler.OnCreateDesktopViewHandlerEvent += OnCreateDesktopViewHandlerEvent;
            _appMainAdapterHandler.OnLogHandlerEvent += OnLogHandlerEvent;
            _appMainAdapterHandler.OnLogOutHandlerEvent += _appMainAdapterHandler_OnLogOutHandlerEvent;
            _appMainAdapterHandler.StartApp();

            var localIPAddress = AppConfiguration.ServiceIPAddress;
            int port = AppConfiguration.ServicePort;

            WebSocketServer server = new WebSocketServer();
            server.NewMessageReceived += Server_NewMessageReceived;
            server.SessionClosed += Server_SessionClosed;
            server.Setup(localIPAddress, port);
            server.Start();
        }



        private void Server_SessionClosed(WebSocketSession session, CloseReason value)
        {
            if (TokenHelper.Session.SessionID == session.SessionID)
                TokenHelper.LogOut();
        }

        private void Server_NewMessageReceived(WebSocketSession session, string value)
        {
            try
            {
                dynamic request = JsonConvert.DeserializeObject(value);
                WebMessageHead cmd = (WebMessageHead)request.msg;
                string desktopId = string.Empty;
                switch (cmd)
                {
                    case WebMessageHead.B_LOGIN:
                        string id = request.id;
                        string password = request.pwd;
                        if (TokenHelper.IsLegalUser(id, password))
                            LoginSuccess(session, id);
                        else
                            session.Send(JsonConvert.SerializeObject(new
                            {
                                code = WebMessageHead.S_ID_OR_KEY_WRONG,
                                message = "账户Id或者密码不正确!"
                            }));
                        break;
                    case WebMessageHead.B_DESKTOP_VIEW_GETFRAME:
                        desktopId = request.desktopId;
                        this.GetDesktopViewFrame(desktopId, request.height, request.width);
                        break;
                    case WebMessageHead.B_MESSAGE_BOX:
                        desktopId = request.desktopId;
                        string msg = request.val;
                        if (_serivceDesktopViews.ContainsKey(desktopId))
                            _appMainAdapterHandler.RemoteMessageBox(_serivceDesktopViews[desktopId].SessionSyncContext, msg, "系统提示", Core.MessageIcon.InforMation);
                        break;
                    case WebMessageHead.B_RESET_DES:
                        desktopId = request.desktopId;
                        string des = request.des;
                        if (_serivceDesktopViews.ContainsKey(desktopId))
                            _appMainAdapterHandler.RemoteSetRemarkInformation(_serivceDesktopViews[desktopId].SessionSyncContext, des);
                        break;
                    case WebMessageHead.B_SYS_SESSION:
                        desktopId = request.desktopId;
                        string state = request.state;
                        if (_serivceDesktopViews.ContainsKey(desktopId))
                            _appMainAdapterHandler.RemoteSetSessionState(_serivceDesktopViews[desktopId].SessionSyncContext, Enum.Parse(typeof(Core.SystemSessionType), state).ConvertTo<Core.SystemSessionType>());
                        break;
                    case WebMessageHead.B_OPEN_URL:
                        desktopId = request.desktopId;
                        string url = request.url;
                        if (_serivceDesktopViews.ContainsKey(desktopId))
                            _appMainAdapterHandler.RemoteOpenUrl(_serivceDesktopViews[desktopId].SessionSyncContext, url);
                        break;
                    case WebMessageHead.B_EXEC_DOWNLOAD:
                        desktopId = request.desktopId;
                        string fileUrl = request.url;
                        if (_serivceDesktopViews.ContainsKey(desktopId))
                            _appMainAdapterHandler.RemoteHttpDownloadExecute(_serivceDesktopViews[desktopId].SessionSyncContext, fileUrl);
                        break;
                    default:
                        session.Send("未识别的指令!");
                        break;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void LoginSuccess(WebSocketSession session, string id)
        {
            if (TokenHelper.Has())
            {
                TokenHelper.Session.Send(JsonConvert.SerializeObject(
                    new
                    {
                        code = WebMessageHead.S_LOGOUT,
                        message = "登出!"
                    }));
            }

            TokenHelper.Session = session;

            GetServicesInfo();
        }

        private void GetServicesInfo()
        {
            _serivceDesktopViews.Clear();

            foreach (var context in _appMainAdapterHandler.SessionSyncContexts)
                _appMainAdapterHandler.RemoteOpenDesktopView(context, true);
        }

        private void GetDesktopViewFrame(string desktopId,int height,int width)
        {
            if (_serivceDesktopViews.ContainsKey(desktopId))
            {
                var view = _serivceDesktopViews[desktopId];
                view.Height = height;
                view.Width = width;
                _appMainAdapterHandler.GetDesktopViewFrame(view.SessionSyncContext);
            }
        }

        private void OnLogHandlerEvent(string log, LogSeverityLevel level)
        {
            Console.WriteLine(level.ToString() + " >> " + log);
        }

        private void _appMainAdapterHandler_OnLogOutHandlerEvent(SessionSyncContext syncContext)
        {
            if (syncContext.KeyDictions.ContainsKey("DesktopViewId") && TokenHelper.Has())
            {
                var desktopId = syncContext.KeyDictions["DesktopViewId"].ToString();
                TokenHelper.Session.Send(JsonConvert.SerializeObject(new
                {
                    code = WebMessageHead.S_CLOSE_SESSION,
                    desktopId = desktopId
                }));
            }
        }

        private IDesktopView OnCreateDesktopViewHandlerEvent(SessionSyncContext syncContext)
        {
            var view = TokenHelper.Has() ? DesktopViewContextsHelper.CreateDesktopView(syncContext, TokenHelper.Session) : null;
            if (!view.IsNull())
                this._appMainAdapterHandler.GetDesktopViewFrame(syncContext);
            else
                return null;

            var des = syncContext.KeyDictions[SysConstants.MachineName].ToString() + "-(" + syncContext.KeyDictions[SysConstants.Remark].ToString() + ")";
            var desktopId = "A" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            TokenHelper.Session.Send(JsonConvert.SerializeObject(new
            {
                code = WebMessageHead.S_LOGIN_SESSION,
                des = des,
                desktopId = desktopId
            }));
            syncContext.KeyDictions.Add("DesktopViewId", desktopId);
            _serivceDesktopViews.Add(desktopId, view);

            return view;
        }

        private void OnProxyNotify(ProxyProviderNotify level, EventArgs arg)
        {
            Console.WriteLine(level.ToString() + " >> " + arg.ToString());
        }
    }
}
