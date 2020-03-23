using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.ServiceCore.Attributes;
using SiMay.Sockets.Tcp.Session;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("文件传输")]
    [ServiceKey(AppJobConstant.REMOTE_REMOTE_UPDATE)]
    [ServiceKey(AppJobConstant.REMOTE_FILE_TRANSMISSION)]
    public class FileTransmissionService : ApplicationRemoteService
    {
        private FileStream _fileStream;

        private long _fileSize = 0;
        public override void SessionInited(TcpSocketSaeaSession session)
        {

        }

        [PacketHandler(MessageHead.S_FILETRAN_CREATE_INOFO)]
        private void CreateFile(TcpSocketSaeaSession session)
        {
            var fileInfor = GetMessageEntity<FileTransmissionCreatePacket>(session);
            _fileSize = fileInfor.FileSize;
            try
            {
                if (fileInfor.FileLocation == FileLocation.ExecuteDirectory)
                {
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileInfor.FileName);
                    var index = 1;
                    while (File.Exists(path))
                    {
                        path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileInfor.FileName) + "_" + index;
                        index++;
                    }
                    _fileStream = new FileStream(path, FileMode.Create);
                }
                else
                {
                    var path = fileInfor.FileName;
                    var index = 1;
                    while (File.Exists(path))
                    {
                        path = fileInfor.FileName + "_" + index;
                        index++;
                    }
                    _fileStream = new FileStream(path, FileMode.Create);
                }

                SendTo(CurrentSession, MessageHead.C_FILETRAN_GET_DATA);
            }
            catch
            {
                var fileName = fileInfor.FileLocation == FileLocation.ExecuteDirectory
                    ? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileInfor.FileName)
                    : fileInfor.FileName;

                SendTo(CurrentSession, MessageHead.C_FILETRAN_TRAN_RESULT,
                    new FileTransmissionResultPacket()
                    {
                        TransmissionSuccess = false,
                        FileName = fileName
                    });
            }
        }

        [PacketHandler(MessageHead.S_FILETRAN_NEXT_DATA)]
        private void DataHandler(TcpSocketSaeaSession session)
        {
            var data = GetMessage(session);
            _fileStream.Write(data, 0, data.Length);

            if (_fileStream.Length == _fileSize)
            {
                var fileName = _fileStream.Name;
                _fileStream.Flush();
                _fileStream.Close();
                SendTo(CurrentSession, MessageHead.C_FILETRAN_TRAN_RESULT,
                    new FileTransmissionResultPacket()
                    {
                        TransmissionSuccess = true,
                        FileName = fileName
                    });

            }
            else SendTo(CurrentSession, MessageHead.C_FILETRAN_GET_DATA);
        }

        public override void SessionClosed()
        {
            try
            {
                _fileStream.Flush();
                _fileStream.Close();
            }
            catch { }
        }
    }
}
