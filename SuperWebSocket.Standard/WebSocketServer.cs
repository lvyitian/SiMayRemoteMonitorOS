using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperWebSocket
{
    /// <summary>
    /// 参考网址： http://blog.csdn.net/wzd24/article/details/1821340
    /// 参考网址   http://blog.csdn.net/wzd24/article/details/1821360
    /// </summary>
    public class WebSocketServer : IWebSocketServer
    {
        protected System.Text.UTF8Encoding decoder = new System.Text.UTF8Encoding();

        #region 私有对象

        bool _islisten = false;
        IPEndPoint _localep = null;
        Socket _listener = null;

        #endregion

        public WebSocketServer()
        {
            InitializationConfigure();
        }

        #region 属性

        private string _id;
        public string Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected DateTime Datetime;

        private WebSocketServerState _state = WebSocketServerState.None;
        public WebSocketServerState State
        {
            get { return _state; }
            set { _state = value; }
        }

        private ServerConfigure _configure = new ServerConfigure();
        public ServerConfigure Configure
        {
            get { return _configure; }
            set { _configure = value; }
        }

        private WebSocketClientCollection _cientCollection = new WebSocketClientCollection();
        internal WebSocketClientCollection ClientCollection
        {
            get { return _cientCollection; }
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        public IWebSocketClient[] Clients
        {
            get { return _cientCollection.ToArray(); }
        }

        private WebSocketConsoleWrite _consoleWrite = new WebSocketConsoleWrite();
        public WebSocketConsoleWrite ConsoleWrite
        {
            get { return _consoleWrite; }
            set { _consoleWrite = value; }
        }


        private WebSocketServerSession _session = null;
        /// <summary>
        /// 开启服务后创建Session
        /// </summary>
        public WebSocketServerSession Session
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

        #endregion

        #region 事件

        public WebSocketEventHandler OnStarted;
        public WebSocketEventHandler OnCreated;
        public WebSocketEventHandler OnLogined;
        public WebSocketEventHandler OnReceive;
        public WebSocketEventHandler OnError;        
        public WebSocketEventHandler OnLogout;
        public WebSocketEventHandler OnClosed;

        protected virtual void DoOnStarted(object sender, WebSocketEventArgs e)
        {
            ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), e.Message, e.DateTime, "WebSocketServer", "");

            WebSocketOperationData startinfo = new WebSocketOperationData();
            startinfo.Code = "(start)";
            startinfo.Request.Func = "start";
            startinfo.Request.User = this.Name;
            startinfo.Result.Message = e.Message;
            startinfo.Result.Name = this.Name;           
            startinfo.Data = e.Data;

            this.Session = new WebSocketServerSession()
            {
                Id = this.Id,
                Name = this.Name,
                Datetime = this.Datetime
            };

            if (this.OnStarted != null)
                this.OnStarted(sender, e);
        }

        protected virtual void DoOnReceive(object sender, WebSocketEventArgs e)
        {
            var data = (WebSocketContextData)((Context)e.Data).Data;

            //由服务端转发
            if (data.ReceiveId != this.Id)
            {
                foreach (WebSocketClient client in this.ClientCollection)
                {
                    if (client.Id == data.ReceiveId)
                    {
                        this.SendData(client, ((Context)e.Data).GetBytes());
                        break;
                    }
                }
            }

            if (this.OnReceive != null)
                this.OnReceive(sender, e);
        }

        protected virtual void DoOnCreated(object sender, WebSocketEventArgs e)
        {
            ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), e.Message, e.DateTime, "WebSocketServer", "");
            if (this.OnCreated != null)
                this.OnCreated(sender, e);
        }

        protected virtual void DoOnError(object sender, WebSocketEventArgs e)
        {
            if (sender is WebSocketClient)
                this.UnConnect((WebSocketClient)sender);
            ConsoleWrite.Message("error", e.Message, e.DateTime, "WebSocketServer", ((Exception)e.Data).StackTrace);
            if (this.OnError != null)
                this.OnError(sender, e);
        }

        protected virtual void DoOnLogin(object sender, WebSocketEventArgs e)
        {
            var data = ((Context)e.Data).Data;
            var current = (WebSocketClient)sender;
            WebSocketOperationData loginInfo = ((WebSocketOperationData)data);
            current.Name = loginInfo.Request.User;
            loginInfo.Result.Success = true;
            loginInfo.Result.Name = loginInfo.Request.User;
            loginInfo.Result.Message = "login success";
            current.Data = loginInfo;

            current.Session = new WebSocketClientSession()
            {
                Id = current.Id,
                Name = current.Name,
                Origin = current.Origin,
                ServerId = this.Session.Id,
                ServerName = this.Session.Name,
                Fun = loginInfo.Request.Func,
                Datetime = e.DateTime
            };

            ClientCollection.Add(current);

            foreach (WebSocketClient client in ClientCollection)
            {
                string SendDataString = "(logined)=>:" + loginInfo.Request.User + "+" + current.Name + "," + loginInfo.Result.Success.ToString().ToLower() + "->:{\"client\":{\"id\":\"" + current.Id + "\",\"name\":\"" + current.Name + "\"},\"clients\":" + ClientCollection.ToUsersJson() + "}";
                byte[] SendDataBuffer = new Context(SendDataString, client.IsDataMasked).GetBytes();
                client.SendData(SendDataBuffer);
            }

            ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), current.Name + " login success", e.DateTime, "WebSocketServer", "");

            if (this.OnLogined != null)
                this.OnLogined(current, new WebSocketEventArgs() { DateTime = e.DateTime, Message = e.Message, Data = loginInfo });
        }

        protected virtual void DoOnLogout(object sender, WebSocketEventArgs e)
        {
            var data = ((Context)e.Data).Data;
            var current = (WebSocketClient)sender;
            foreach (WebSocketClient client in ClientCollection)
            {
                if (client == null)
                    continue;

                string SendDataString = "(logouted)=>:" + client.Id + "," + this.Id + "->:{\"client\":{\"id\":\"" + current.Id + "\",\"name\":\"" + current.Name + "\"}}";
                byte[] SendDataBuffer = new Context(SendDataString, client.IsDataMasked).GetBytes();
                if (client.State == WebSocketClientState.Connected)
                    client.SendData(SendDataBuffer);
            }
            ClientCollection.Remove(current);

            ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), current.Name + " logout success", e.DateTime, "WebSocketServer", "");

            WebSocketOperationData logoutinfo = (WebSocketOperationData)current.Data;
            logoutinfo.Code = "(logout)";              
            logoutinfo.Result.Message = "logout success";          
            logoutinfo.Result.Success = true;

            if (this.OnLogout != null)
                this.OnLogout(current, new WebSocketEventArgs() { DateTime = e.DateTime, Message = "logout success", Data = logoutinfo });
        }

        protected virtual void DoOnClosed(object sender, WebSocketEventArgs e)
        {
            ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), "server " + this.Name + " close", e.DateTime, "WebSocketServer", "");

            WebSocketOperationData closeinfo = new WebSocketOperationData();
            closeinfo.Code = "(close)";
            closeinfo.Request.Func = "close";
            closeinfo.Request.User = this.Name;           
            closeinfo.Result.Name = this.Name;           
            closeinfo.Result.Success = true;
            closeinfo.Result.Message = "server close";
            closeinfo.Data = e.Data;           

            if (this.OnClosed != null)
                this.OnClosed(sender, new WebSocketEventArgs() { DateTime = e.DateTime, Message = e.Message, Data = closeinfo });
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化配置
        /// </summary>
        private void InitializationConfigure()
        {
            this.Id = "wss" + Guid.NewGuid().ToString().Replace("-", "");
            ConsoleWrite.WriteLevel = WebSocketConsoleWriteLevel.debug;
            ConsoleWrite.WriteType = WebSocketConsoleWriteType.console;
            Configure.IPAddress = GetLocalmachineIPAddress();
            Configure.Port = 2345;            
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        private IPAddress GetLocalmachineIPAddress()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);

            this.Name = strHostName;

            foreach (IPAddress ip in ipEntry.AddressList)
            {
                //IP V4
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip;
                if (ip.AddressFamily == AddressFamily.Unix)
                    return ip;
            }

            return ipEntry.AddressList[0];
        }

        /// <summary>
        /// 异步方法开始接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void BeginReceiveCallBack(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;

            var ReceiveCount = client.ConnectionSocket.EndReceive(ar);//调用这个函数来结束本次接收并返
            var ReceiveData = decoder.GetString(client.ReceivedDataBuffer, 0, ReceiveCount);

            byte[] last8Bytes = new byte[8];
            if (ReceiveCount > 8)
            {
                Array.Copy(client.ReceivedDataBuffer, ReceiveCount - 8, last8Bytes, 0, 8);
            }
            string last8BytesString = decoder.GetString(last8Bytes);

            bool IsDataMasked = false;
            string Origin, AcceptKey = "";          

            byte[] SendDataBuffer = decoder.GetBytes(WebSocketContractBuilder.BuildResponseContract(this.Id, this.Name, client.Id, client.Name, ReceiveData, out Origin, out AcceptKey, out IsDataMasked));
            client.Origin = Origin;
            client.AcceptKey = AcceptKey;
            client.IsDataMasked = IsDataMasked;             
             
            //异步方式调用
            if (client.ConnectionSocket.Connected)
                client.ConnectionSocket.BeginSend(SendDataBuffer, 0, SendDataBuffer.Length, SocketFlags.None, new AsyncCallback(EndSendCallBack), client);

        }        

        /// <summary>
        /// 异步方法结束发送回调
        /// </summary>
        /// <param name="ar"></param>
        private void EndSendCallBack(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            client.ConnectionSocket.EndSend(ar);
            Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);
            if (client.ConnectionSocket.Connected)
                client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, new AsyncCallback(Read), client);
        }

        /// <summary>
        /// 异步方法接收数据
        /// </summary>
        /// <param name="ar"></param>
        private void Read(IAsyncResult ar)
        {
            var client = (WebSocketClient)ar.AsyncState;
            try
            {
                client.ConnectionSocket.EndReceive(ar);               

                if (!client.ConnectionSocket.Connected)
                    return;              
                
                var Context = new Context(client.ReceivedDataBuffer, client.IsDataMasked);

                var data = Context.Data;
                switch (data.Code)
                {
                    case "(login)":
                        DoOnLogin(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Data = Context, Message = "login..." });
                        break;
                    case "(logout)":
                        DoOnLogout(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Data = Context, Message = "logout..." });
                        break;
                    case "(file)":                  
                    case "(data)":
                    case "(image)": 
                    case "(message)":                        
                        DoOnReceive(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Data = Context, Message = "receive..." });
                        break;
                }              

                Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);

                if (client.ConnectionSocket.Connected)
                    client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, new AsyncCallback(Read), client);

            }
            catch(Exception ex)
            {
                Array.Clear(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length);

                if (!client.ConnectionSocket.Connected)
                {
                    this.UnConnect(client);
                    var data = new Context("(logout)=>:" + this.Id + "," + client.Id + "->:{\"client\":{\"id\":\"" + client.Id + "\",\"name\":\"" + client.Name + "\"}}", client.IsDataMasked);
                    DoOnLogout(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = data });
                }
                else
                {
                    DoOnError(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
                }                
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="client"></param>
        private void UnConnect(WebSocketClient current)
        {
            current.State = WebSocketClientState.Disconnected;

            foreach (WebSocketClient client in ClientCollection)
            {
                if (client == null)
                    continue;

                string SendDataString = "(logouted)=>:" + client.Id + "," + this.Id + "->:{\"client\":{\"id\":\"" + current.Id + "\",\"name\":\"" + current.Name + "\"}}";
                byte[] SendDataBuffer = new Context(SendDataString, client.IsDataMasked).GetBytes();
                if (client.State == WebSocketClientState.Connected)
                    client.SendData(SendDataBuffer);
            }

            current.ConnectionSocket.Close();
            current.ConnectionSocket.Dispose();
            this.ClientCollection.Remove(current);

            WebSocketOperationData logoutinfo = (WebSocketOperationData)current.Data;
            logoutinfo.Code = "(logout)";
            logoutinfo.Result.Message = "logout success";
            logoutinfo.Result.Success = true;

            if (this.OnLogout != null)
                this.OnLogout(current, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "logout success", Data = logoutinfo });

        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="SendDataBuffer"></param>
        private void SendData(WebSocketClient Client, byte[] SendDataBuffer)
        {
            Client.SendData(SendDataBuffer);            
        }

        /// <summary>
        /// 广播数据
        /// </summary>
        /// <param name="SendDataBuffer"></param>
        private void SendDataToALL(byte[] SendDataBuffer)
        {
            foreach (WebSocketClient client in this.ClientCollection)
            {
                client.SendData(SendDataBuffer);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Id">Client Id</param>
        /// <param name="Type">如:data,message,file</param>
        /// <param name="Data">数据</param>
        protected void SendData(string Id, string Type, byte[] Data)
        {
            foreach (WebSocketClient client in ClientCollection)
            {
                if (client.Id == Id
                    && client.Name != "guest"
                    && client.State == WebSocketClientState.Connected)
                {
                    byte[] FuncBuffer = decoder.GetBytes("(" + Type + ")=>:" + client.Id + "," + this.Id + "->:");
                    int length = FuncBuffer.Length + Data.Length;

                    Head Head = new Head(true, false, false, false, 1, false, length);
                    byte[] HeadBuffer = Head.GetBytes();

                    byte[] SendDataBuffer = new byte[HeadBuffer.Length + FuncBuffer.Length + Data.Length];
                    Array.Copy(HeadBuffer, 0, SendDataBuffer, 0, HeadBuffer.Length);
                    Array.Copy(FuncBuffer, 0, SendDataBuffer, HeadBuffer.Length, FuncBuffer.Length);
                    Array.Copy(Data, 0, SendDataBuffer, HeadBuffer.Length + FuncBuffer.Length, Data.Length);

                    var d = new Context(SendDataBuffer, client.IsDataMasked);
                    client.SendData(d.GetBytes());
                }
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        protected void Stop()
        {
            _islisten = false;
            if (_listener != null)
            {                
                _listener.Close();
            }      
            this.State = WebSocketServerState.Stoped;
        }

        /// <summary>
        /// 释放
        /// </summary>
        protected void Dispose()
        {
            if (_listener != null)
            {
                _listener.Close();
                _listener.Dispose();
            }
            foreach (IWebSocketClient client in ClientCollection)
                client.Close();
            this.ClientCollection.Clear();
            GC.Collect();
            this.State = WebSocketServerState.Disposed;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 开启
        /// </summary>
        public void Start()
        {
            Char char1 = Convert.ToChar(65533);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _localep = new IPEndPoint(_configure.IPAddress, _configure.Port);

            try
            {

                _islisten = true;
                _listener.Bind(_localep);
                _listener.Listen(_configure.ListenCount);

                new System.Threading.Thread(() => {

                    while (_islisten)
                    {
                        try
                        {
                            WebSocketClient client = new WebSocketClient(_listener.Accept());
                            try
                            {
                                client.Id = "wsc" + Guid.NewGuid().ToString().Replace("-", ""); //分配一个ID
                                client.Name = "guest";
                                client.ServerId = this.Id;
                                client.ServerName = this.Name;
                                client.State = WebSocketClientState.Connected;
                                client.Datetime = DateTime.Now;

                                client.ConnectionSocket.BeginReceive(client.ReceivedDataBuffer, 0, client.ReceivedDataBuffer.Length, SocketFlags.None, BeginReceiveCallBack, client);
                                this.DoOnCreated(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "" + client.Name + " create success" });

                            }
                            catch (Exception ex)
                            {
                                this.DoOnError(client, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
                            }

                        }
                        catch (Exception ex)
                        {
                            if (this.State == WebSocketServerState.Started && _islisten)
                            {
                                this.DoOnError(this, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
                            }
                        }
                    }

                }).Start();

                ConsoleWrite.Message(ConsoleWrite.WriteLevel.ToString(), "IP: " + this.Configure.IPAddress + " , 端口：" + this.Configure.Port + " 已开启监听...", DateTime.Now, "WebSocketServer", "");

                this.Datetime = DateTime.Now;
                this.State = WebSocketServerState.Started;
                this.DoOnStarted(this, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = DateTime.Now.ToString() + " 服务 ( IP: " + this.Configure.IPAddress + " , 端口：" + this.Configure.Port + " ) 已开启监听...", Data = 2340 });

            }
            catch (Exception ex)
            {
                this.DoOnError(this, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = ex.Message, Data = ex });
            }

        }      

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            Stop();            
            Dispose();
            this.DoOnClosed(this, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "server close", Data = 2344 });
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Data"></param>
        public void SendData(string Id, byte[] Data)
        {
            this.SendData(Id, "data", Data);
        }      

        #endregion

    }
}
