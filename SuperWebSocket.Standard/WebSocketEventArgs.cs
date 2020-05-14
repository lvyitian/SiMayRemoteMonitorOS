using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    public class WebSocketEventArgs : EventArgs
    {
        public string Message { get; set; }
        public object Data { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class WebSocketProgressEventArgs : EventArgs
    {
        public long Total { get; set; }
        public long Value { get; set; }
        public string Precent { get; set; }
    }

    public delegate bool WebSocketEventHandler(object sender, WebSocketEventArgs e);

    public delegate void WebSocketProgressEventHandler(object sender, WebSocketProgressEventArgs e);

}
