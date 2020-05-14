using System;
using System.Collections.Generic;
using System.Text;

namespace SuperWebSocket
{
    #region  Context Part Member

    internal class Head
    {
        private bool _fin; //结束
        private bool _rsv1; //备用
        private bool _rsv2; //备用
        private bool _rsv3; //备用
        private bool _hasmask;// 掩码
        private sbyte _operationcode;// Opcode  4bit 帧类型 操作码
        private sbyte _payloadlength;// Payload len 有效长度

        private byte[] _maskcode = new byte[0]; //掩码
        private byte[] _extend = new byte[0]; //扩展

        internal const int Maxlength = 65536; // 网页端传输数据最大长度

        /// <summary>
        /// 结束标识
        /// </summary>
        internal bool FIN { get { return _fin; } }

        internal bool RSV1 { get { return _rsv1; } }

        internal bool RSV2 { get { return _rsv2; } }

        internal bool RSV3 { get { return _rsv3; } }

        /// <summary>
        /// 操作码
        /// </summary>
        internal sbyte OperationCode { get { return _operationcode; } }

        /// <summary>
        /// 是否有掩码
        /// </summary>
        internal bool HasMask { get { return _hasmask; } }

        /// <summary>
        /// 掩码
        /// </summary>
        internal byte[] MaskCode { get { return _maskcode; } }

        /// <summary>
        /// 长度
        /// </summary>
        internal sbyte PayloadLength { get { return _payloadlength; } }

        /// <summary>
        /// 扩展
        /// </summary>
        internal byte[] Extend { get { return _extend; } }

        internal int Length
        {
            get
            {
                return 2 + _extend.Length + MaskCode.Length;
            }
        }

        //接收封装数据
        internal Head(byte[] DataBuffer)
        {
            if (DataBuffer.Length < 2)
                throw new Exception("无效的数据头.");

            //第一个字节
            _fin = (DataBuffer[0] & 0x80) == 0x80; //0x80 = 128
            _rsv1 = (DataBuffer[0] & 0x40) == 0x40; //0x40 = 64
            _rsv2 = (DataBuffer[0] & 0x20) == 0x20; //0x20 = 32
            _rsv3 = (DataBuffer[0] & 0x10) == 0x10; //0x10 = 16
            _operationcode = (sbyte)(DataBuffer[0] & 0x0f); //0x0f = 15

            //第二个字节
            _hasmask = (DataBuffer[1] & 0x80) == 0x80; //0x80 = 128
            _payloadlength = (sbyte)(DataBuffer[1] & 0x7f);  //0x0f = 127

            switch (_payloadlength)
            {
                case 126:
                    _extend = new byte[2];
                    Buffer.BlockCopy(DataBuffer, 2, _extend, 0, 2);
                    break;
                case 127:
                    _extend = new byte[8];
                    Buffer.BlockCopy(DataBuffer, 2, _extend, 0, 8);
                    break;
            }

            //是否有掩码
            if (_hasmask)
            {
                _maskcode = new byte[4];
                Buffer.BlockCopy(DataBuffer, _extend.Length + 2, _maskcode, 0, 4);
            }

        }


        //发送封装数据
        internal Head(bool fin, bool rsv1, bool rsv2, bool rsv3, sbyte opcode, bool hasmask, int length)
        {
            //第一个字节
            _fin = fin;
            _rsv1 = rsv1;
            _rsv2 = rsv2;
            _rsv3 = rsv3;
            _operationcode = opcode;

            _hasmask = hasmask;
            _extend = new byte[0];
            _maskcode = new byte[0];

            if (length < 126)
            {
                _extend = new byte[0];
                _payloadlength = (sbyte)(length);
            }
            else if (length < Maxlength)
            {
                _extend = new byte[2];
                _extend[0] = (byte)(length / 256);
                _extend[1] = (byte)(length % 256);
                _payloadlength = (sbyte)(126);
            }
            else
            {
                _extend = new byte[8];
                int left = length;
                int unit = 256;
                for (int i = 7; i > 1; i--)
                {
                    _extend[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
                _payloadlength = (sbyte)(127);
            }

            //第二个字节           
            if (_hasmask)
            {
                _maskcode = new byte[4];
                _maskcode[0] = (byte)0x80;
                _maskcode[1] = (byte)(length / 256);
                _maskcode[2] = (byte)(length % 256);
                _maskcode[3] = (byte)0x10;
            }


        }


        internal byte[] GetBytes()
        {
            byte[] buffer = new byte[2 + _extend.Length + MaskCode.Length];

            if (_fin) buffer[0] ^= 0x80;
            if (_rsv1) buffer[0] ^= 0x40;
            if (_rsv2) buffer[0] ^= 0x20;
            if (_rsv3) buffer[0] ^= 0x10;

            buffer[0] ^= (byte)_operationcode;

            if (_hasmask) buffer[1] ^= 0x80;

            buffer[1] ^= (byte)_payloadlength;

            Buffer.BlockCopy(_extend, 0, buffer, 2, _extend.Length);
            Buffer.BlockCopy(_maskcode, 0, buffer, 2 + _extend.Length, _maskcode.Length);

            return buffer;
        }


        internal string Text
        {
            get { return Encoding.UTF8.GetString(GetBytes()); }
        }

    }

    internal class Body
    {
        private int _length;
        private byte[] _buffer;

        private string Trim(string input)
        {
            string rvl = string.Empty;
            if (!string.IsNullOrEmpty(input))
            {
                rvl = input.Trim();
            }
            return rvl;
        }

        internal Body(byte[] content)
        {
            _buffer = content;
            _length = _buffer.Length;
        }

        internal Body(string content)
        {
            _buffer = Encoding.UTF8.GetBytes(content);
            _length = _buffer.Length;
        }

        internal byte[] GetBytes()
        {
            return _buffer;
        }

        internal string Text
        {
            get
            {
                return Encoding.UTF8.GetString(_buffer);
            }
        }

        internal int Length { get { return _length; } }
    }

    #endregion

    public class Context
    {
        internal System.Text.Encoding decoder = new System.Text.UTF8Encoding();

        internal Head Head { get; set; }

        internal Body Body { get; set; }

        internal WebSocketConsoleWrite writer = new WebSocketConsoleWrite();

        public ContextData Data { get; set; }

        public long BodyBufferLength(int length)
        {
            int n = 1;
            long len = 0;
            for (int i = length - 1; i >= 0; i--)
            {
                len += (int)this.Head.Extend[i] * n;
                n *= 256;
            }         
            return len;
        }

        public byte[] Mask(byte[] data, byte[] mask)
        {
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }
            return data;
        }
               
        public byte[] GetBytes()
        {             
            byte[] buffer = new byte[Head.Length + Body.Length];
            Buffer.BlockCopy(Head.GetBytes(), 0, buffer, 0, Head.Length);
            Buffer.BlockCopy(Body.GetBytes(), 0, buffer, Head.Length, Body.Length);
            return buffer;
        }

        public Context(string Content, bool IsDataMasked)
        {
            //与网页端交互不能有掩码
            //byte[] _bodyBuffer=System.Text.Encoding.UTF8.GetBytes(Content);
            //this.Head = new Head(true, false, false, false, 1, IsDataMasked, _bodyBuffer.Length);
            //if (IsDataMasked)
            //{
            //    _bodyBuffer = Mask(_bodyBuffer, this.Head.MaskCode);
            //}
            //this.Body = new Body(_bodyBuffer);  


            byte[] _bodyBuffer = System.Text.Encoding.UTF8.GetBytes(Content);
            this.Head = new Head(true, false, false, false, 1, false, _bodyBuffer.Length);
            //if (IsDataMasked)
            //{
            //    _bodyBuffer = Mask(_bodyBuffer, this.Head.MaskCode);
            //}
            this.Body = new Body(_bodyBuffer);

        }

        public Context(byte[] DataBuffer, bool IsDataMasked)
        {
            #region  消息头

            this.Head = new Head(DataBuffer);

            #endregion

            #region 消息体

            byte[] _bodyBufferContent;

            long _bodyBufferLength = 0;

            switch (this.Head.Extend.Length)
            {
                case 0:
                    _bodyBufferLength = Head.PayloadLength;
                    _bodyBufferContent = new byte[_bodyBufferLength];
                    Buffer.BlockCopy(DataBuffer, this.Head.Extend.Length + this.Head.MaskCode.Length + 2, _bodyBufferContent, 0, _bodyBufferContent.Length);
                    break;
                case 2:                    
                    _bodyBufferLength = BodyBufferLength(2);
                    _bodyBufferContent = new byte[_bodyBufferLength];  //最大 1024 * 100;                 
                    Buffer.BlockCopy(DataBuffer, this.Head.Extend.Length + this.Head.MaskCode.Length + 2, _bodyBufferContent, 0, _bodyBufferContent.Length);
                    break;
                case 8:                    
                    _bodyBufferLength = BodyBufferLength(8);
                    _bodyBufferContent = new byte[_bodyBufferLength];                     
                    Buffer.BlockCopy(DataBuffer, this.Head.Extend.Length + this.Head.MaskCode.Length + 2, _bodyBufferContent, 0, _bodyBufferContent.Length);
                    break;
                default:
                    _bodyBufferContent = new byte[0];
                    break;
            }
            if (this.Head.HasMask)
            {
                _bodyBufferContent = Mask(_bodyBufferContent, this.Head.MaskCode);
            }

            this.Body = new Body(_bodyBufferContent);

            #endregion

            #region 数据体

            this.Data = new ContextData();

            //(login)=>:用户名+密码，功能->:数据  返回用户列表
            //(logined)=>:用户名+昵称，功能->:数据  返回用户列表
            //(message/data)=>:接收者，发送者->:内容
            //(logout)=>:用户名+昵称，功能->:数据 

            string content = this.Body.Text;

            if (content.IndexOf("=>:") != -1 && content.IndexOf("->:") != -1)
            {
                var data = (object)null;

                short code_pos_index = (short)content.IndexOf("=>:");
                short resp_pos_index = (short)content.IndexOf("->:");
                string code = content.Substring(0, code_pos_index);
                string resp = content.Substring(code_pos_index + 3, resp_pos_index - code_pos_index - 3);

                string[] strings = resp.Split(',');

                switch (code)
                {
                    case "(login)":
                        data = content.Substring(resp_pos_index + 3);
                        string[] loginInfo = strings[0].Split('+');
                        this.Data = new WebSocketOperationData()
                        {
                            Request = new WebSocketRequestData()
                            {
                                Code = code,
                                User = loginInfo[0].ToString(),
                                Password = loginInfo[1].ToString(),
                                Func = strings[1].ToString()
                            },
                            Result = new WebSocketResultData()
                            {
                                Success = false,
                                Message = "request login"
                            },
                            Data = data
                        };
                        break;
                    case "(logined)":
                        data = content.Substring(resp_pos_index + 3);
                        string[] loginedInfo = strings[0].Split('+');
                        this.Data = new WebSocketOperationData()
                        {
                            Request = new WebSocketRequestData()
                            {
                                Code = code,
                                User = loginedInfo[0].ToString()
                            },
                            Result = new WebSocketResultData()
                            {
                                Name = loginedInfo[1].ToString(),
                                Success = Convert.ToBoolean(strings[1].ToString()),
                                Message = strings[1] == "true" ? "logined" : "refuse"
                            },
                            Data = data
                        };
                        break;
                    case "(logout)":
                        data = content.Substring(resp_pos_index + 3);
                        string[] logoutInfo = strings[0].Split('+');
                        this.Data = new WebSocketOperationData()
                        {
                            Request = new WebSocketRequestData()
                            {
                                Code = code,
                                User = logoutInfo[0].ToString(),
                                Func = strings[1].ToString()
                            },
                            Result = new WebSocketResultData()
                            {
                                Name = logoutInfo[1].ToString(),
                                Message = logoutInfo[0].ToString() + " request logout"
                            },
                            Data = data
                        };
                        break;
                    case "(logouted)":
                        data = content.Substring(resp_pos_index + 3);
                        this.Data = new WebSocketContextData()
                        {
                            ReceiveId = strings[0].ToString(),
                            SendId = strings[1].ToString(),
                            Data = data
                        };
                        break;
                    case "(message)":
                        data = content.Substring(resp_pos_index + 3);
                        this.Data = new WebSocketContextData()
                        {
                            ReceiveId = strings[0].ToString(),
                            SendId = strings[1].ToString(),
                            Data = data
                        };
                        break;
                    case "(data)":
                        data = new byte[this.Body.Length - resp_pos_index - 3];
                        int dataIndex = resp_pos_index + 3 + 2;
                        switch (this.Head.PayloadLength)
                        {
                            case 126:
                                dataIndex = resp_pos_index + 3 + 2 + 2;
                                break;
                            case 127:
                                dataIndex = resp_pos_index + 3 + 2 + 2 + 6;
                                break;
                        }
                        Array.Copy(DataBuffer, dataIndex, (byte[])data, 0, ((byte[])data).Length);
                        this.Data = new WebSocketContextData()
                        {
                            ReceiveId = strings[0].ToString(),
                            SendId = strings[1].ToString(),
                            Data = data
                        };
                        break;
                    case "(image)":

                        int imagebufferIndex = content.IndexOf("->>:");
                        string imageInfo = content.Substring(resp_pos_index + 3, imagebufferIndex - resp_pos_index - 3);
                        string[] imageStrings = imageInfo.Split(',');

                        int imageSize = 0;
                        string imageName = string.Empty;
                        string imageType = string.Empty;

                        foreach (string value in imageStrings)
                        {
                            if (value.StartsWith("name="))
                            {
                                imageName = value.Substring("name=".Length);
                            }
                            if (value.StartsWith("type="))
                            {
                                imageType = value.Substring("type=".Length);
                            }
                            if (value.StartsWith("size="))
                            {
                                imageSize = Int32.Parse(value.Substring("size=".Length));
                            }
                        }


                        data = new byte[imageSize];

                        WebSocketImageData wsImageData = new WebSocketImageData(imageName, imageType, imageSize);
                        
                        int imageIndex = resp_pos_index + 3 + 2;
                        switch (this.Head.PayloadLength)
                        {
                            case 126:
                                imageIndex = resp_pos_index + 3 + 2 + wsImageData.ImageInfoMaxLength + 2;
                                break;
                            case 127:
                                imageIndex = resp_pos_index + 3 + 2 + 2 + wsImageData.ImageInfoMaxLength + 6;
                                break;
                        }

                        Array.Copy(DataBuffer, imageIndex, (byte[])data, 0, ((byte[])data).Length);
                                               
                        wsImageData.ReceiveId = strings[0].ToString();
                        wsImageData.SendId = strings[1].ToString();
                        wsImageData.Data = data;

                        this.Data = new WebSocketContextData()
                        {
                            ReceiveId = strings[0].ToString(),
                            SendId = strings[1].ToString(),
                            Data = wsImageData
                        };

                        break;

                    case "(file)":

                        int filebufferIndex = content.IndexOf("->>:");
                        string fileInfo = content.Substring(resp_pos_index + 3, filebufferIndex - resp_pos_index - 3);
                        string[] fileStrings = fileInfo.Split(',');

                        long fileSize = 0;                       
                        long fileStart = 0;
                        long fileEnd = 0;
                        string fileName = string.Empty;
                        string fileType = string.Empty;
                        string fileState = string.Empty;
                        string fileSendId = string.Empty;
                        string fileReceiveId = string.Empty;
                        string fileSendInfo = string.Empty;
                        string fileReceiveInfo = string.Empty;

                        foreach (string value in fileStrings)
                        {
                            if (value.StartsWith("name="))
                            {
                                fileName = value.Substring("name=".Length);
                            }
                            if (value.StartsWith("type="))
                            {
                                fileType = value.Substring("type=".Length);
                            }
                            if (value.StartsWith("size="))
                            {
                                fileSize = Int64.Parse(value.Substring("size=".Length));
                            }                            
                            if (value.StartsWith("start="))
                            {
                                fileStart = Int64.Parse(value.Substring("start=".Length));
                            }
                            if (value.StartsWith("end="))
                            {
                                fileEnd = Int64.Parse(value.Substring("end=".Length));
                            }
                            if (value.StartsWith("state="))
                            {
                                fileState = value.Substring("state=".Length);
                            }
                            if (value.StartsWith("sendid="))
                            {
                                fileSendId = value.Substring("sendid=".Length);
                            }
                            if (value.StartsWith("receiveid="))
                            {
                                fileReceiveId = value.Substring("receiveid=".Length);
                            }
                            if (value.StartsWith("sendinf="))
                            {
                                fileSendInfo = value.Substring("sendinf=".Length);
                            }
                            if (value.StartsWith("receiveinf="))
                            {
                                fileReceiveInfo = value.Substring("receiveinf=".Length);
                            }
                        }

                        data = new byte[fileEnd - fileStart];

                        WebSocketFileData wsFileData = new WebSocketFileData(fileName, fileType, fileSize);
                        wsFileData.State = wsFileData.GetFileState(fileState);
                        wsFileData.Start = fileStart;
                        wsFileData.End = fileEnd;
                        wsFileData.SendInfo = fileSendInfo;
                        wsFileData.ReceiveInfo = fileReceiveInfo;

                        int fileIndex = resp_pos_index + 3 + 2;
                        switch (this.Head.PayloadLength)
                        {
                            case 126:
                                fileIndex = resp_pos_index + 3 + 2 + wsFileData.FileInfoMaxLength + 2;
                                break;
                            case 127:
                                fileIndex = resp_pos_index + 3 + 2 + 2 + wsFileData.FileInfoMaxLength + 6;
                                break;
                        }

                        Array.Copy(DataBuffer, fileIndex, (byte[])data, 0, ((byte[])data).Length);

                        wsFileData.ReceiveId = fileReceiveId;
                        wsFileData.SendId = fileSendId;               
                     
                        wsFileData.Data = data;

                        this.Data = new WebSocketContextData()
                        {
                            ReceiveId = strings[0].ToString(),
                            SendId = strings[1].ToString(),
                            Data = wsFileData
                        };

                        break;

                }

                this.Data.Code = code;

            }

            #endregion
        }

    }

    public class ContextData
    {
        public string Code { get; set; }
        public virtual object Data { get; set; }
    }

    public class WebSocketContextData : ContextData
    {
        public string SendId { get; internal set; }
        public string ReceiveId { get; internal set; }
        protected string GetBytesSize(long length)
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
    }

    public class WebSocketResultData : ContextData
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Name { get; set; }
    }

    public class WebSocketRequestData : ContextData
    {
        public string Func { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class WebSocketOperationData : ContextData
    {
        public WebSocketRequestData Request { get; set; }
        public WebSocketResultData Result { get; set; }

        public WebSocketOperationData()
        {
            this.Request = new WebSocketRequestData();
            this.Result = new WebSocketResultData();
        }
    }

    public class WebSocketLoginResultData : WebSocketResultData
    {
        public WebSocketClientSession client { get; set; }
        public WebSocketClientSession[] clients { get; set; }

        private WebSocketClientSession[] getSections(string input)
        {
            List<WebSocketClientSession> result = new List<WebSocketClientSession>();
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\{.*?\}", System.Text.RegularExpressions.RegexOptions.Compiled);
            var match = regex.Match(input);
            while (match.Success)
            {
                result.Add(getSection(match.Groups[0].Value));
                match = match.NextMatch();
            }
            return result.ToArray();
        }

        private WebSocketClientSession getSection(string input)
        {
            WebSocketClientSession client = new WebSocketClientSession();
            input = input.Trim().Substring(1, input.Length - 2);
            string[] array = input.Split(',');
            foreach (string arr in array)
            {
                string[] values = arr.Split(':');
                if (values[0] == "\"id\"")
                {
                    client.Id = values[1].Substring(1, values[1].Length - 2);
                }
                else if (values[0] == "\"name\"")
                {
                    client.Name = values[1].Substring(1, values[1].Length - 2);
                }
            }
            return client;
        }

        public WebSocketLoginResultData(ContextData data)
        {
            string json = data.Data.ToString();
            if (json.IndexOf("\"client\":") != -1)
            {
                string _jsonkey = json.Substring(json.IndexOf("\"client\":{"), json.IndexOf("}") - json.IndexOf("\"client\":{") + 1);
                string _jsonvalue = _jsonkey.Substring("\"client\":".Length);
                this.client = getSection(_jsonvalue);
            }
            if (json.IndexOf("\"clients\":") != -1)
            {
                string _jsonkey = json.Substring(json.IndexOf("clients\":["), json.IndexOf("]") - json.IndexOf("clients\":[") + 1);
                string _jsonvalue = _jsonkey.Substring("clients\":".Length);
                this.clients = getSections(_jsonvalue);
            }
            this.Data = data;
            this.Code = data.Code;
        }
    }

    public class WebSocketMessageData : WebSocketContextData
    {

    }

    public class WebSocketImageData : WebSocketContextData
    {
        public string Name { get; set; }
        public string Format { get; set; }

        private int _imgSize;
        public int Size
        {
            get
            {
                return _imgSize;
            }
            private set
            {
                _imgSize = value;
            }
        }

        private byte[] _imgData = null;
        public override object Data
        {
            get
            {
                return _imgData;
            }
            set
            {
                _imgData = (byte[])value;
            }
        }

        public string ImageSize
        {
            get { return GetBytesSize(this.Size); }
        }
                 
        public int ImageInfoMaxLength
        {
            get
            {                
                return System.Text.Encoding.UTF8.GetBytes("name=" + this.Name + ",type=" + this.Format + ",size=" + _imgSize + "->>:").Length;
            }
        }

        public int ImageDataMaxLength
        {
            get
            {
                return 2 * 1024 * 1024; //2 M
            }
        }

        public WebSocketImageData(string Name, string Format, byte[] Data)
        {
            this.Name = Name;
            this.Format = Format;           
            this.Data = Data;
            this.Size = Data.Length;
            this.Code = "(image)";
        }

        public WebSocketImageData(string Name, string Format, int Size)
        {
            this.Name = Name;
            this.Format = Format;
            this.Size = Size;
            this.Data = new byte[Size];
            this.Code = "(image)";
        }

        public byte[] GetBytes()
        {
            string FileInfoDescrib = "name=" + this.Name + ",type=" + this.Format + ",size=" + _imgSize + "->>:";

            byte[] FileInfoBuffer = System.Text.Encoding.UTF8.GetBytes(FileInfoDescrib);
            byte[] FileDataBuffer = (byte[])this._imgData;  
            byte[] DataBuffer = new byte[this.ImageInfoMaxLength + FileDataBuffer.Length];

            Array.Copy(FileInfoBuffer, 0, DataBuffer, 0, FileInfoBuffer.Length);
            Array.Copy(FileDataBuffer, 0, DataBuffer, this.ImageInfoMaxLength, FileDataBuffer.Length);

            return DataBuffer;
        }

    }

    public enum WebSocketFileState
    {
        None,
        Begin,
        Refuse,
        Receive,
        Start,
        Transferring,
        Finish,
        End
    }

    public class WebSocketFileData : WebSocketContextData
    {        
        public string Name { get; set; }
        public string Format { get; set; }
        public long Start { get; set; }   
        public long End { get; set; }
        public WebSocketFileState State { get; set; }
        public string SendInfo { get; set; }
        public string ReceiveInfo { get; set; }

        private long _fileSize;
        public long Size
        {
            get
            {
                return _fileSize;
            }
            set
            {
                _fileSize = value;
            }
        }

        private byte[] _fileData = new byte[0];
        public override object Data
        {
            get
            {
                return _fileData;
            }
            set
            {
                _fileData = (byte[])value;
            }
        }     

        public string FileSize
        {
            get { return GetBytesSize(this.Size); }
        }

        public int FileInfoMaxLength
        {
            get
            {
                return System.Text.Encoding.UTF8.GetBytes("name=" + this.Name + ",type=" + this.Format + ",size=" + this.Size + ",start=" + this.Start + ",end=" + this.End + ",state=" + this.State.ToString() + ",sendid=" + this.SendId + ",receiveid=" + this.ReceiveId + ",sendinf=" + this.SendInfo + ",receiveinf=" + this.ReceiveInfo + "->>:").Length;
            }
        }

        public int FileDataMaxLength
        {
            get
            {
                return 2 * 1024 * 1024; //2 G 2 * 1024 * 1024 * 1024
            }
        }

        internal int BlockBufferLength = 2 * 1024; //2 KB        

        public WebSocketFileData(string Name, string Format, byte[] Data)
        {
            this.Name = Name;
            this.Format = Format;
            this.Data = Data;          
            this.Code = "(file)";          
            this.State = WebSocketFileState.None;
        }

        public WebSocketFileData(string Name, string Format, long Size)
        {
            this.Name = Name;
            this.Format = Format;
            this.Size = Size;       
            this.Code = "(file)";          
            this.State = WebSocketFileState.None;
        }

        public byte[] GetBytes()
        {
            string FileInfoDescrib = "name=" + this.Name + ",type=" + this.Format + ",size=" + this.Size + ",start=" + this.Start + ",end=" + this.End + ",state=" + this.State.ToString() + ",sendid=" + this.SendId + ",receiveid=" + this.ReceiveId + ",sendinf=" + this.SendInfo + ",receiveinf=" + this.ReceiveInfo + "->>:";

            byte[] FileInfoBuffer = System.Text.Encoding.UTF8.GetBytes(FileInfoDescrib);
            byte[] FileDataBuffer = (byte[])this._fileData;
            byte[] DataBuffer = new byte[this.FileInfoMaxLength + FileDataBuffer.Length];

            Array.Copy(FileInfoBuffer, 0, DataBuffer, 0, FileInfoBuffer.Length);
            Array.Copy(FileDataBuffer, 0, DataBuffer, this.FileInfoMaxLength, FileDataBuffer.Length);

            return DataBuffer;
        }

        public WebSocketFileState GetFileState(string state)
        {
            return (WebSocketFileState)Enum.Parse(typeof(WebSocketFileState), state);
        }

    }

   

}
