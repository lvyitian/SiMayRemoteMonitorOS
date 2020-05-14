using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public class SuperWebSocketServer : WebSocketServer
    {
        #region  私有方法
        protected override void DoOnReceive(object sender, WebSocketEventArgs e)
        {
            base.DoOnReceive(sender, e);
            var data = ((Context)e.Data).Data;
            switch (data.Code)
            {
                case "(message)":
                    this.DoOnMessage(sender, new WebSocketEventArgs() { Message = e.Message, DateTime = e.DateTime, Data = data });
                    break;
                case "(image)":
                    this.DoOnImage(sender, new WebSocketEventArgs() { Message = e.Message, DateTime = e.DateTime, Data = data });
                    break;
                case "(file)":
                    WebSocketFileData wsFileData = (WebSocketFileData)data.Data;
                    if (wsFileData.SendId == this.Id)
                    {
                        switch (wsFileData.State)
                        {
                            case WebSocketFileState.Receive:
                                DoOnBeinSendFile(sender, new WebSocketEventArgs() { DateTime = e.DateTime, Message = "", Data = wsFileData });
                                break;
                            case WebSocketFileState.Refuse:
                                DoOnFinishSendFile(sender, new WebSocketEventArgs() { DateTime = e.DateTime, Message = "", Data = wsFileData });
                                break;
                        }
                    }
                    if (wsFileData.ReceiveId == this.Id)
                    {

                    }
                    break;
            }
        }

        #endregion

        #region 事件

        public WebSocketEventHandler OnStoped;

        public WebSocketEventHandler OnMessage;

        public WebSocketEventHandler OnImage;

        private void DoOnMessage(object sender, WebSocketEventArgs e)
        {
            if (this.OnMessage != null)
                this.OnMessage(sender, e);
        }

        private void DoOnImage(object sender, WebSocketEventArgs e)
        {            
            if (this.OnImage != null)
                this.OnImage(sender, e);
        }

        private void DoOnStoped(object sender, WebSocketEventArgs e)
        {
            if (this.OnStoped != null)
                this.OnStoped(sender, e);
        }


        public WebSocketEventHandler OnBeinSendFile;

        private void DoOnBeinSendFile(object sender, WebSocketEventArgs e)
        {
            if (this.OnBeinSendFile != null)
                this.OnBeinSendFile(sender, e);

            WebSocketFileData wsFileData = (WebSocketFileData)e.Data;
            wsFileData.State = WebSocketFileState.Transferring;

            byte[] sendDataBuffer = null;

            long start = 0;
            long end = 0;
            long sended = 0;

            using (System.IO.FileStream fs = new System.IO.FileStream(wsFileData.SendInfo, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                long total = fs.Length;
                while (end < total)
                {
                    end = start + wsFileData.FileDataMaxLength;
                    if (end > total) end = total;
                    sendDataBuffer = new byte[end - start];

                    wsFileData.Start = start;
                    wsFileData.End = end;
                    wsFileData.Data = sendDataBuffer;
                    if (end == total)
                    {
                        wsFileData.State = WebSocketFileState.Finish;
                    }

                    start = start + wsFileData.FileDataMaxLength;
                    sended += sendDataBuffer.Length;

                    DoOnReportSendFile(this, new WebSocketProgressEventArgs() { Value = sended, Total = total });

                    System.Threading.Thread.Sleep(100);

                }

                DoOnFinishSendFile(sender, e);

            }

        }

        public WebSocketEventHandler OnFinishSendFile;

        private void DoOnFinishSendFile(object sender, WebSocketEventArgs e)
        {
            if (this.OnFinishSendFile != null)
                this.OnFinishSendFile(sender, e);
        }

        public WebSocketProgressEventHandler OnReportSendFile;

        private void DoOnReportSendFile(object sender, WebSocketProgressEventArgs e)
        {
            int value = (int)(e.Value * 1.0 / e.Total * 100);
            string precent = (e.Value * 1.0 / e.Total * 100).ToString("N2") + " %";
            if (this.OnReportSendFile != null)
            {
                this.OnReportSendFile(sender, new WebSocketProgressEventArgs()
                {
                    Total = 100,
                    Value = value,
                    Precent = precent
                });
            }
        }

        /**************************接收文件***************************/

        private void DoOnBeginReceiveFile(object sender, WebSocketEventArgs e)
        {

        }

        private void DoOnFinishReceiveFile(object sender, WebSocketEventArgs e)
        {

        }

        private void DoOnReportReceiveFile(WebSocketEventArgs e)
        {

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

        public void SendFile(string Id, string Name, string Type, string File)
        {
            var client = this.ClientCollection.GetWebSocketClient(Id);
            if (client == null) return;
            var socket = ((WebSocketClient)client).ConnectionSocket;
            if (!socket.Connected) return;
            System.IO.FileInfo fi = new System.IO.FileInfo(File);
            if (!fi.Exists) return;

            WebSocketFileData data = new WebSocketFileData(Name, Type, fi.Length);
            byte[] FileDataBuffer = new byte[data.BlockBufferLength];

            data.SendId = this.Id;
            data.ReceiveId = client.Id;
            data.SendInfo = fi.FullName;

            data.Data = FileDataBuffer;
            data.State = WebSocketFileState.Begin;

            this.SendData(data.ReceiveId, "file", data.GetBytes());

        }

        public new void Stop() 
        {
            base.Stop();
            DoOnStoped(this, new WebSocketEventArgs() { DateTime = DateTime.Now, Message = "server stop", Data = 4321 });
        }

        #endregion

    }
}
