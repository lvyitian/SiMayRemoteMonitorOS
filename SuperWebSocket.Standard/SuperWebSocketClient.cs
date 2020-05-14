using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperWebSocket
{
    public class SuperWebSocketClient : WebSocketClient
    {
        #region 公共属性      


        #endregion

        #region 构造方法

        public SuperWebSocketClient() : base()
        {

        }

        public SuperWebSocketClient(Socket Socket, long BufferLength) : base(Socket, BufferLength)
        {

        }

        public SuperWebSocketClient(Socket Socket) : base(Socket, 1024 * 1024 * 10)
        {

        }

        #endregion

        #region 私有方法

        

        #endregion

        #region 事件

        public WebSocketEventHandler OnImage;

        private void DoOnImage(WebSocketEventArgs e)
        {            
            if (this.OnImage != null)
                this.OnImage(this, e);
        }

        public WebSocketEventHandler OnData;

        private void DoOnData(WebSocketEventArgs e)
        {
            if (this.OnData != null)
                this.OnData(this, e);
        }

        public WebSocketEventHandler OnMessage;

        private void DoOnMessage(WebSocketEventArgs e)
        {
            if (this.OnMessage != null)
                this.OnMessage(this, e);
        }
        
        public WebSocketEventHandler OnBeginReceiveFile;

        private void DoOnBeginReceiveFile(WebSocketEventArgs e)
        {
            if (this.OnBeginReceiveFile != null)
            {
                bool result = this.OnBeginReceiveFile(this, e);
                WebSocketFileData wsFileData = (WebSocketFileData)e.Data;
                if (result)
                {
                    wsFileData.State = WebSocketFileState.Receive;
                }
                else
                {
                    wsFileData.State = WebSocketFileState.Refuse;
                }
                this.SendData(wsFileData.SendId, "file", wsFileData.GetBytes());
            }
        }

        public WebSocketEventHandler OnFinishReceiveFile;

        private void DoOnFinishReceiveFile(WebSocketEventArgs e)
        {
            if (this.OnFinishReceiveFile != null)
            {
                var result = this.OnFinishReceiveFile(this, e);
            }
        }        

        private void DoOnReportReceiveFile(WebSocketEventArgs e)
        {

        }

        protected override void DoOnReceive(WebSocketEventArgs e)
        {
            ContextData data = (ContextData)e.Data;
            switch (data.Code)
            {
                case "(message)":
                    DoOnMessage(new WebSocketEventArgs() { Message = e.Message, DateTime = e.DateTime, Data = data });
                    break;
                case "(data)":
                    DoOnData(new WebSocketEventArgs() { Message = e.Message, DateTime = e.DateTime, Data = data });
                    break;
                case "(image)":
                    DoOnImage(new WebSocketEventArgs() { Message = e.Message, DateTime = e.DateTime, Data = data });
                    break;
                case "(file)":
                    WebSocketFileData fileData = (WebSocketFileData)data.Data;
                    switch (fileData.State) {
                        case WebSocketFileState.Begin:
                            DoOnBeginReceiveFile(new WebSocketEventArgs() { DateTime = e.DateTime, Message = "begin", Data = fileData });
                            break;
                        case WebSocketFileState.Transferring:
                            break;
                        case WebSocketFileState.Finish:
                            break;
                    }
                    break;
            }
        }

        #endregion

        #region 公共方法  

        public string GetBytesSize(long length)
        {
            long GBT = 1024 * 1024 * 1024; //G BT
            long MBT = 1024 * 1024;
            long KBT = 1024;
            string result = length + " BT";
            if (length > GBT)
            {
                result = (length * 1.0 / GBT).ToString("N2") + " G BT";
            }
            else if (length > MBT)
            {
                result = (length * 1.0 / MBT).ToString("N2") + " M BT";
            }
            else if (length > KBT)
            {
                result = (length * 1.0 / KBT).ToString("N2") + " K BT";
            }
            else
            {
                result = length + " BT";
            }
            return result;
        }

        public void SendMessage(string Id, string Message)
        {
            this.SendData(Id, "message", decoder.GetBytes(Message));
        }

        public void SendImage(string Id, string Name, string Type, byte[] Image)
        {
            this.SendData(Id, "image", new WebSocketImageData(Name, Type, Image).GetBytes());
        }

        #endregion

    }
}
