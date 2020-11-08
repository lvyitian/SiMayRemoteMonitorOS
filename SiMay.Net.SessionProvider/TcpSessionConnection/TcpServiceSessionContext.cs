using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public class TcpServiceSessionContext : SessionProviderContext
    {
        public TcpServiceSessionContext(TcpSocketSaeaSession session)
        {
            CurrentSession = session;
            session.AppTokens = new object[] { this };
        }
        /// <summary>
        /// 发送长度
        /// </summary>
        public override int SendTransferredBytes => CurrentSession.SendTransferredBytes;

        /// <summary>
        /// 接受长度
        /// </summary>
        public override int ReceiveTransferredBytes => CurrentSession.ReceiveBytesTransferred;


        private byte[] _decompressData;
        /// <summary>
        /// 完成缓冲区
        /// </summary>
        public override byte[] CompletedBuffer
        {
            get
            {
                //缓存解压数据，防止重复调用造成性能低下
                if (_decompressData.IsNull())
                {
                    var waitDecompressData = ProxyProtocolConstructionHelper.TakeHeadAndMessage(CurrentSession.CompletedBuffer);
                    _decompressData = GZipHelper.Decompress(waitDecompressData);
                }
                return _decompressData;
            }
        }

        public void OnProcess()
        {
            //数据到达后清除缓存数据
            _decompressData = null;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void SendAsync(byte[] data, int offset, int length)
        {
            var reoffsetData = GZipHelper.Compress(data, offset, length);
            CurrentSession.SendAsync(ProxyProtocolConstructionHelper.WrapAccessId(reoffsetData, ApplicationConfiguartion.Options.AccessId));
        }

        /// <summary>
        /// 关闭会话
        /// </summary>
        public override void SessionClose(bool notify = true) => CurrentSession.Close(notify);

        /// <summary>
        /// Socket设置
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        public override void SetSocketOptions(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            if (optionValue is byte[] bytes)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, bytes);
            else if (optionValue is int num)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, num);
            else if (optionValue is bool b)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, b);
            else
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }
    }
}
