using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperWebSocket
{
    public class WebSocketClient : IWebSocketClient
    {
        internal Socket ConnectionSocket { get; set; }
        internal byte[] ReceivedDataBuffer { get; set; }

        internal string ServerId = string.Empty;
        internal string ServerName = string.Empty;

        internal DateTime Datetime;

        protected System.Text.Encoding decoder = new System.Text.UTF8Encoding();

        #region 公共属性

        private string _id;
        public string Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _acceptKey;
        public string AcceptKey
        {
            get { return _acceptKey; }
            internal set { _acceptKey = value; }
        }

        private bool _isDataMasked;
        public bool IsDataMasked
        {
            get { return _isDataMasked; }
            internal set { _isDataMasked = value; }
        }

        private string _origin;
        public string Origin
        {
            get { return _origin; }
            internal set { _origin = value; }
        }

        private string _encode="utf8";
        public string Encode
        {
            get { return _encode; }
            private set
            {
                _encode = Trim(value);
                decoder = System.Text.Encoding.GetEncoding(_encode);
            }
        }

        private WebSocketConsoleWrite _consoleWrite = new WebSocketConsoleWrite();
        public WebSocketConsoleWrite ConsoleWrite
        {
            get { return _consoleWrite; }
            set { _consoleWrite = value; }
        }

        private WebSocketClientState _state = WebSocketClientState.None;
        public WebSocketClientState State
        {
            get
            {
                if (!this.ConnectionSocket.Connected)
                    _state = WebSocketClientState.Disconnected;
                return _state;
            }
            set { _state = value; }
        }

        private WebSocketOperationData _data = new WebSocketOperationData();
        internal WebSocketOperationData Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private ClientConfigure _configure = new ClientConfigure();
        public ClientConfigure Configure
        {
            get { return _configure; }
            set { _configure = value; }
        }

        private WebSocketClientSession _session = null;
        /// <summary>
        /// 连接成功后创建Session
        /// </summary>
        public WebSocketClientSession Session
        {
            get
            {
                return _session;
            }
            set
            {
                _session = value;
            }
        } 

        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region  构造方法

        /// <summary>
        /// 创建一个客户端连接
        /// </summary>
        public WebSocketClient()
        {
            this.IsDataMasked = true;
            this.ReceivedDataBuffer = new byte[ReceivedDataBufferLength.Value];
            this.ConsoleWrite.WriteType = WebSocketConsoleWriteType.console;
        }

        #region 服务端创建        

        internal WebSocketClient(Socket Socket, long BufferLength)
        {
            this.IsDataMasked = true;
            this.ConnectionSocket = Socket;
            this.ReceivedDataBuffer = new byte[BufferLength];
            this.ConsoleWrite.WriteType = WebSocketConsoleWriteType.console;
        }

        internal WebSocketClient(Socket Socket) : this(Socket, ReceivedDataBufferLength.Value)
        {

        }

        #endregion

        #endregion

        #region  私有方法

        private string Trim(object value)
        {
            return value == null ? "" : value.ToString().Trim();
        }

        private void BeginConnect(IPAddress IPAddress, short Port)
        {
            this._configure.Port = Port;
            this._configure.IPAddress = IPAddress;
           
            ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConnectionSocket.Connect(new System.Net.IPEndPoint(IPAddress, Port)); //配置服务器IP与端口 
            byte[] SendDataBuffer = decoder.GetBytes(WebSocketContractBuilder.BuildRequestContract(IPAddress.ToString(), Port.ToString()));
            if (ConnectionSocket.Connected)
                this.State = WebSocketClientState.Connected;
            ConnectionSocket.BeginSend(SendDataBuffer, 0, SendDataBuffer.Length, SocketFlags.None, EndConnect, this);

            this.Datetime = DateTime.Now;
        }

        private void EndConnect(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            client.ConnectionSocket.EndSend(ar);
            
            Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);
            if (client.ConnectionSocket.Connected)
                client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, BeginLogin, client);
        }

        private void BeginLogin(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            var ReceiveCount = client.ConnectionSocket.EndReceive(ar);
            var ReceiveData = decoder.GetString(client.ReceivedDataBuffer, 0, ReceiveCount);

            string[] lines = ReceiveData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.IndexOf("Content-Type") != -1)
                {
                    Encode = line.Substring(line.IndexOf("Content-Type") + "Content-Type".Length + 1);
                }
                if (line.IndexOf("Sec-WebSocket-ClientId") != -1)
                {
                    Id = line.Substring(line.IndexOf("Sec-WebSocket-ClientId") + "Sec-WebSocket-ClientId".Length + 1);
                }
                if (line.IndexOf("Sec-WebSocket-ServerId") != -1)
                {
                    ServerId = line.Substring(line.IndexOf("Sec-WebSocket-ServerId") + "Sec-WebSocket-ServerId".Length + 1);
                }
                if (line.IndexOf("Sec-WebSocket-ServerName") != -1)
                {
                    ServerName = line.Substring(line.IndexOf("Sec-WebSocket-ServerName") + "Sec-WebSocket-ServerName".Length + 1);
                }
                if (line.IndexOf("Sec-WebSocket-Accept") != -1)
                {
                    AcceptKey = line.Substring(line.IndexOf("Sec-WebSocket-Accept") + "Sec-WebSocket-Accept".Length + 1);
                }
            }

            Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);
            client.State = WebSocketClientState.Connected;

            if (!string.IsNullOrEmpty(AcceptKey))
            {
                byte[] SendDataBuffer = new Context("(login)=>:" + this.Data.Request.User + "+" + this.Data.Request.Password + "," + this.Data.Request.Func + "->:" + this.Data.Data + "", client.IsDataMasked).GetBytes();
                if (client.ConnectionSocket.Connected)
                    client.ConnectionSocket.BeginSend(SendDataBuffer, 0, SendDataBuffer.Length, SocketFlags.None, new AsyncCallback(EndLogin), client);

                DoOnConnected(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "client connected", Data=this.Data.Request });
            }

        }

        private void EndLogin(IAsyncResult ar)
        {
            var client = (SuperWebSocketClient)ar.AsyncState;
            client.ConnectionSocket.EndSend(ar);
            Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);

            this.Session = new WebSocketClientSession()
            {
                Id = this.Id,
                Name = this.Name,
                Origin = this.Origin,
                ServerId = this.ServerId,
                ServerName = this.ServerName,
                Fun = this.Data.Request.Func,
                Datetime = DateTime.Now
            };

            if (client.ConnectionSocket.Connected)
                client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, ReceiveLoginCallBack, client);
        }

        private void ReceiveLoginCallBack(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            try
            {
                var ReceiveCount = client.ConnectionSocket.EndReceive(ar);
                //var ReceiveData = decoder.GetString(client.ReceivedDataBuffer, 0, ReceiveCount);
                var data = new Context(client.ReceivedDataBuffer, client.IsDataMasked);
                var result = ((WebSocketOperationData)data.Data).Result;
                if (result.Success)
                {                    
                    this.Name = result.Name;
                    this.Data.Result = result;
                   
                    data.Data.Data = new WebSocketLoginResultData(data.Data);
                    this.DoOnLogined(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "login success", Data = data.Data });

                    Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);
                    if (client.ConnectionSocket.Connected)
                        client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, Read, client);
                }
            }
            catch (Exception ex)
            {
                DoOnError(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
            }
        }

        private void Read(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            try
            {
                var ReceiveCount = client.ConnectionSocket.EndReceive(ar);
                //var ReceiveData = decoder.GetString(client.ReceivedDataBuffer, 0, ReceiveCount);
                var data = new Context(client.ReceivedDataBuffer, client.IsDataMasked);

                switch (data.Data.Code)
                {
                    case "(logined)":
                        data.Data.Data = new WebSocketLoginResultData((ContextData)data.Data);
                        this.DoOnLogined(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "logined", Data = data.Data });
                        break;                   
                    case "(data)":
                    case "(file)":
                    case "(image)":
                    case "(message)":
                        this.DoOnReceive(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "data transport", Data = data.Data });
                        break;
                    case "(logouted)":
                        data.Data.Data = new WebSocketLoginResultData(data.Data);
                        this.DoOnLogout(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "logout", Data = data.Data });
                        break;
                }

                Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);
                if (client.ConnectionSocket.Connected)
                    client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, Read, client);
            }
            catch (Exception ex)
            {
                this.DoOnError(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
            }
        }

        private void EndSend(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            if (client.ConnectionSocket.Connected)
                client.ConnectionSocket.EndSend(ar);
        }

        internal void SendData(byte[] SendDataBuffer)
        {
            if (ConnectionSocket.Connected)
                ConnectionSocket.BeginSend(SendDataBuffer, 0, SendDataBuffer.Length, SocketFlags.None, new AsyncCallback(EndSend), this);
        }

        internal void SendData(string Id, string Type, byte[] Data)
        {
            byte[] FuncBuffer = decoder.GetBytes("(" + Type + ")=>:" + Id + "," + this.Id + "->:");
            int length = FuncBuffer.Length + Data.Length;

            Head Head = new Head(true, false, false, false, 1, false, length);
            byte[] HeadBuffer = Head.GetBytes();

            byte[] SendDataBuffer = new byte[HeadBuffer.Length + FuncBuffer.Length + Data.Length];
            Array.Copy(HeadBuffer, 0, SendDataBuffer, 0, HeadBuffer.Length);
            Array.Copy(FuncBuffer, 0, SendDataBuffer, HeadBuffer.Length, FuncBuffer.Length);
            Array.Copy(Data, 0, SendDataBuffer, HeadBuffer.Length + FuncBuffer.Length, Data.Length);

            if (ConnectionSocket.Connected)
                ConnectionSocket.BeginSend(SendDataBuffer, 0, SendDataBuffer.Length, SocketFlags.None, new AsyncCallback(EndSend), this);
        }

        internal void Dispose()
        {
            //this.ConnectionSocket.Disconnect(true);
            this.ConnectionSocket.Close();
            this.ConnectionSocket.Dispose();
        }

        #endregion

        #region 事件

        public WebSocketEventHandler OnConnected;
        public WebSocketEventHandler OnLogined;
        public WebSocketEventHandler OnReceive;
        public WebSocketEventHandler OnLogout;
        public WebSocketEventHandler OnError;
        public WebSocketEventHandler OnClosed;

        protected virtual void DoOnError(WebSocketEventArgs e)
        {
            this.ConsoleWrite.Message("error", e.Message, DateTime.Now, "WebSocketClient", ((Exception)e.Data).StackTrace);
            Array.Clear(ReceivedDataBuffer, 0, ReceivedDataBuffer.Length);
            if (this.OnError != null)
                this.OnError(this, e);
        }

        protected virtual void DoOnConnected(WebSocketEventArgs e)
        {
            if (this.OnConnected != null)
                this.OnConnected(this, e);
        }

        protected virtual void DoOnLogined(WebSocketEventArgs e)
        {
            this.ConsoleWrite.Message(this.ConsoleWrite.WriteLevel.ToString(), e.Message, DateTime.Now, "WebSocketClient", "");

            if (this.OnLogined != null)
                this.OnLogined(this, e);
        }

        protected virtual void DoOnLogout(WebSocketEventArgs e)
        {
            WebSocketLoginResultData datajson = (WebSocketLoginResultData)((ContextData)e.Data).Data;
            WebSocketClientSession client = (WebSocketClientSession)datajson.client;
            if (client.Id == this.Id)
            {
                this.Dispose();
                this.DoOnClosed(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = client.Name + " logout success", Data = e.Data });
            }
            else
            {
                this.ConsoleWrite.Message(this.ConsoleWrite.WriteLevel.ToString(), e.Message, DateTime.Now, "WebSocketClient", "");
                if (this.OnLogout != null)
                    this.OnLogout(this, e);
            }
        }

        protected virtual void DoOnClosed(WebSocketEventArgs e)
        {
            this.ConsoleWrite.Message(this.ConsoleWrite.WriteLevel.ToString(), e.Message, DateTime.Now, "WebSocketClient", "");
            if (this.OnClosed != null)
                this.OnClosed(this, e);
        }

        protected virtual void DoOnReceive(WebSocketEventArgs e)
        {
            if (this.OnReceive != null)
                this.OnReceive(this, e);
        }

        #endregion

        #region 公共方法       

        public void Close()
        {
            var data = new Context("(logout)=>:" + this.Data.Request.User + "+" + this.Data.Result.Name + ",logout->:" + null + "", this.IsDataMasked).GetBytes();
            SendData(data);
        }

        /// <summary>
        /// 登陆并注册功能
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="fun"></param>
        /// <param name="arg"></param>
        public void Connect(string ip, string port, string user, string password, string func, string data)
        {
            this.Data = new WebSocketOperationData();
            this.Data.Code = "(login)";
            this.Data.Request.User = user;
            this.Data.Request.Password = password;
            this.Data.Request.Func = func;
            this.Data.Data = data;

            try
            {
                this.BeginConnect(IPAddress.Parse(ip), short.Parse(port));
            }
            catch (Exception ex)
            {
                this.DoOnError(new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
            }

        }

        #endregion

    }
}
