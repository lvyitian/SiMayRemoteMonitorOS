using System;
using System.Security.Cryptography;
using System.Text;

namespace SuperWebSocket
{   
    internal class WebSocketStringBuilder
    {
        private string _stringValue = string.Empty;

        internal void AppendLine()
        {
            _stringValue += Environment.NewLine;
        }

        internal void AppendLine(string stringline)
        {
            _stringValue += stringline + Environment.NewLine;
        }

        internal void AppendLine(string format,params object[] values)
        {
            _stringValue += string.Format(format,values) + Environment.NewLine;
        }

        internal string Value
        {
            get
            {
                return _stringValue;
            }
        }

    }

    internal class WebSocketKeyBuilder
    {
        #region 加密，生成安全码相关

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1">4位</param>
        /// <param name="key2">4位</param>
        /// <param name="origin">8位</param>
        /// <returns></returns>
        internal static String BuildSecurityHash08(string key1, string key2, string origin)
        {
            byte[] concatenatedKeys = new byte[16];
            System.Text.UTF8Encoding decoder = new System.Text.UTF8Encoding();
            concatenatedKeys = decoder.GetBytes(string.Concat(key1, key2, origin));

            // MD5 Hash  
            System.Security.Cryptography.MD5 MD5Service = System.Security.Cryptography.MD5.Create();
            byte[] acceptKeyBuffer = MD5Service.ComputeHash(concatenatedKeys);

            return decoder.GetString(acceptKeyBuffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        internal static String BuildSecurityHash09(string key1, string key2)
        {
            String secWebSocketAccept = String.Empty;
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key1 + key2));
            secWebSocketAccept = Convert.ToBase64String(sha1Hash);
            return secWebSocketAccept;
        }

        #endregion
    }

    internal class WebSocketContractBuilder
    {
        internal const String Key = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        internal static string BuildResponseContract(string ServerId, string ServerName, string ClientId, string ClientName, string Context, out string Origin, out string AcceptKey, out bool IsDataMasked)
        {
            string[] ClientHandshakeLines = Context.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            IsDataMasked = false;
            AcceptKey = Origin = "";

            if (Context.Contains("Sec-WebSocket-Version"))
            {
                //获取acceptKey               
                foreach (string Line in ClientHandshakeLines)
                {
                    if (Line.Contains("Sec-WebSocket-Key:"))
                        AcceptKey = WebSocketKeyBuilder.BuildSecurityHash09(Line.Substring(Line.IndexOf(":") + 2), Key);
                    if (Line.Contains("Origin:"))
                        Origin = Line.Substring(Line.IndexOf(":") + 2);
                }

                IsDataMasked = true;
            }

            WebSocketStringBuilder contract = new WebSocketStringBuilder();
            contract.AppendLine("HTTP/1.1 101 Switching Protocols");
            contract.AppendLine("Upgrade:WebSocket");
            contract.AppendLine("Connection:Upgrade");
            contract.AppendLine("Sec-WebSocket-Accept:{0}", AcceptKey);
            contract.AppendLine("Sec-WebSocket-ServerId:{0}", ServerId);
            contract.AppendLine("Sec-WebSocket-ServerName:{0}", ServerName);
            contract.AppendLine("Sec-WebSocket-ClientId:{0}", ClientId);
            contract.AppendLine("Sec-WebSocket-ClientName:{0}", ClientName);
            contract.AppendLine("Sec-WebSocket-Response:{0}", true);
            contract.AppendLine("Access-Control-Allow-Credentials:{0}", true);
            contract.AppendLine("Content-Type:{0}", "utf-8");
            contract.AppendLine();

            return contract.Value;
        }

        internal static string BuildRequestContract(string Ip, string Port)
        {
            WebSocketStringBuilder contract = new WebSocketStringBuilder();
            contract.AppendLine("GET /services HTTP/1.1");
            contract.AppendLine("Host: {0}:{1}", Ip, Port);
            contract.AppendLine("Connection: Upgrade");
            contract.AppendLine("Pragma: no-cache");
            contract.AppendLine("Cache-Control: no-cache");
            contract.AppendLine("Upgrade: websocket");
            contract.AppendLine("Origin: ws://{0}:{1}/client", Ip, Port);
            contract.AppendLine("Sec-WebSocket-Version: 13");
            contract.AppendLine("Accept-Encoding: gzip, deflate, sdch");
            contract.AppendLine("Accept-Language: zh-CN,zh;q=0.8");
            contract.AppendLine("User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36");
            contract.AppendLine("Sec-WebSocket-Key: gj73Ujw9bVqIRhLbkYpkTw==");
            contract.AppendLine("Sec-WebSocket-Extensions: permessage-deflate; client_max_window_bits");
            contract.AppendLine();

            return contract.Value;
        }

    }

}
