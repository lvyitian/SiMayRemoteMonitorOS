using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public class FileTransmissionAdapterHandler : ApplicationAdapterHandler
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        public event Action<FileTransmissionAdapterHandler, long, long> TransmissionSendEventHandler;

        /// <summary>
        /// 传输结束
        /// </summary>
        public event Action<FileTransmissionAdapterHandler, string> TransmissionEndEventHandler;

        private IFileStream _fileStream;
        private byte[] _readBuffer = new byte[1024 * 100];
        public void SendFileStream(IFileStream stream, string fileName)
        {
            SendTo(CurrentSession, MessageHead.S_FILETRAN_CREATE_INOFO,
                new FileTransmissionCreatePacket()
                {
                    FileLocation = FileLocation.ExecuteDirectory,
                    FileName = fileName,
                    FileSize = stream.Length
                });

            _fileStream = stream;
        }

        [PacketHandler(MessageHead.C_FILETRAN_GET_DATA)]
        private void GetNextDataHandler(SessionProviderContext session)
        {
            var lenght = _fileStream.Read(_readBuffer, 0, _readBuffer.Length);
            SendTo(CurrentSession, MessageHead.S_FILETRAN_NEXT_DATA, _readBuffer.Copy(0, lenght));

            this.TransmissionSendEventHandler?.Invoke(this, lenght, _fileStream.Length);
        }

        [PacketHandler(MessageHead.C_FILETRAN_TRAN_RESULT)]
        private void TransmssionResult(SessionProviderContext session)
        {
            var fileName = GetMessageEntity<FileTransmissionResultPacket>(session).FileName;
            this.TransmissionEndEventHandler?.Invoke(this, fileName);
            _fileStream.Close();
        }

        public override void SessionClosed(SessionProviderContext session)
        {
            _fileStream.Close();
            base.SessionClosed(session);
        }
    }
}
