using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.Packets;
using SiMay.Core.Packets.FileManager;
using SiMay.Net.SessionProvider;
using SiMay.Serialize.Standard;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class RemoteFileAdapterHandler : ApplicationAdapterHandler
    {
        /// <summary>
        /// 文件列表项
        /// </summary>
        public event Action<RemoteFileAdapterHandler, FileItem[], string, bool, string> OnFileItemsEventHandler;

        /// <summary>
        /// 快速导航文件列表项
        /// </summary>
        public event Action<RemoteFileAdapterHandler, FileItem[]> OnFileTreeItemsEventHandler;

        /// <summary>
        /// 文件删除完成
        /// </summary>
        public event Action<RemoteFileAdapterHandler, string[]> OnFileDeteledFinishEventHandler;

        /// <summary>
        /// 远程异常信息
        /// </summary>
        public event Action<RemoteFileAdapterHandler, DateTime, string, string, string> OnRemoteExceptionEventHandler;

        /// <summary>
        /// 粘贴完成
        /// </summary>
        public event Action<RemoteFileAdapterHandler, string[]> OnPasterFinishEventHandler;

        /// <summary>
        /// 打开文本完成
        /// </summary>
        public event Action<RemoteFileAdapterHandler, string, bool> OnOpenTextEventHandler;

        /// <summary>
        /// 文件重命名完成
        /// </summary>
        public event Action<RemoteFileAdapterHandler, string, string, bool> OnFileNameRenameFinishEventHandler;

        /// <summary>
        /// 文件夹创建完成
        /// </summary>
        public event Action<RemoteFileAdapterHandler, bool> OnDirectoryCreateFinishEventHandler;

        /// <summary>
        /// 文件传输进度
        /// </summary>
        public event Action<RemoteFileAdapterHandler, FileTransferFlag, string, long, long> OnFileTransferProgressEventHandler;

        /// <summary>
        /// 传输任务状态信号
        /// </summary>
        public TransferTaskFlage TransferTaskFlage { get; set; } = TransferTaskFlage.Allow;

        private const int FILE_BUFFER_SIZE = 1024 * 512;
        private volatile bool _isWorkSessionOfLines = true; //session是否在线状态
        private TransferMode? _transferMode = null;

        /// <summary>
        /// 内置版本号的自动事件
        /// </summary>
        private AwaitAutoResetEvent _workerStreamEvent = new AwaitAutoResetEvent(false);
        private ManualResetEvent _sessionOfLinesEvent = new ManualResetEvent(true);
        private ManualResetEvent _filesTriggerEvent = new ManualResetEvent(false);
        private Queue<DirectoryFileItem> _filesQueue = new Queue<DirectoryFileItem>();
        /// <summary>
        /// 获取所有驱动器
        /// </summary>
        public void GetRemoteDriveItems()
        {
            SendTo(CurrentSession, MessageHead.S_FILE_GET_DRIVES);
        }

        /// <summary>
        /// 获取远程文件信息
        /// </summary>
        /// <param name="path"></param>
        public void GetRemoteFiles(string path)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_GET_FILES,
                new FileListPack()
                {
                    FilePath = path
                });
        }

        /// <summary>
        /// 获取系统特殊目录文件信息
        /// </summary>
        /// <param name="specialFolder"></param>
        public void GetRemoteSystemFoldFiles(Environment.SpecialFolder specialFolder)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_REDIRION,
                new FileRedirectionPath()
                {
                    SpecialFolder = specialFolder
                });
        }

        /// <summary>
        /// 文件项接收
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_FILE_LIST)]
        private void FixedRemoteFileItems(SessionProviderContext session)
        {
            var fileItems = GetMessageEntity<FileListItemsPack>(session);
            this.OnFileItemsEventHandler?.Invoke(this, fileItems.FileList, fileItems.Path, fileItems.IsSccessed, fileItems.Message);
        }

        /// <summary>
        /// 远程异常信息
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_ERROR_INFO)]
        private void WriteExceptionLog(SessionProviderContext session)
        {
            var log = GetMessageEntity<FileExceptionPack>(session);
            this.OnRemoteExceptionEventHandler?.Invoke(this, log.OccurredTime, log.TipMessage, log.ExceptionMessage, log.StackTrace);
        }

        /// <summary>
        /// 粘贴文件
        /// </summary>
        /// <param name="root"></param>
        /// <param name="files"></param>
        public void RemoteFilePaster(string root, string[] files)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_FILE_PASTER,
                    new FileCopyPack()
                    {
                        TargetDirectoryPath = root,
                        FileNames = files
                    });
        }

        /// <summary>
        /// 粘贴完成
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_COPY_FINISH)]
        private void RefreshDirectory(SessionProviderContext session)
        {
            var pack = GetMessageEntity<FileCopyFinishPack>(session);
            this.OnPasterFinishEventHandler?.Invoke(this, pack.ExceptionFileNames);
        }

        /// <summary>
        /// 打开远程文本文件
        /// </summary>
        /// <param name="path"></param>
        public void RemoteOpenText(string path)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_OPEN_TEXT,
                                        new FileOpenTextPack()
                                        {
                                            FileName = path
                                        });
        }

        /// <summary>
        /// 打开文本完成
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_TEXT)]
        private void OpenRemoteText(SessionProviderContext session)
        {
            var text = GetMessageEntity<FileTextPack>(session);
            this.OnOpenTextEventHandler?.Invoke(this, text.Text, text.IsSuccess);
        }

        /// <summary>
        /// 删除远程文件
        /// </summary>
        /// <param name="files"></param>
        public void RemoteDeleteFiles(string[] files)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_DELETE,
                    new FileDeletePack()
                    {
                        FileNames = files
                    });
        }

        /// <summary>
        /// 文件删除完成
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_DELETE_FINISH)]
        private void DeleteFinishHandler(SessionProviderContext session)
        {
            var files = GetMessageEntity<FileDeleteFinishPack>(session);
            this.OnFileDeteledFinishEventHandler?.Invoke(this, files.DeleteFileNames);
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="noCallback">false = 回调</param>
        public void RemoteCreateDirectory(string path, bool noCallback = false)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_CREATE_DIR,
                new FileCreateDirectoryPack()
                {
                    DirectoryName = path,
                    NoCallBack = noCallback
                });
        }

        /// <summary>
        /// 文件夹创建完成
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_CREATEF_DIR_FNISH)]
        private void DirectoryCreateFinishHandler(SessionProviderContext session)
        {
            var response = GetMessageEntity<FileCreateDirectoryFinishPack>(session);
            this.OnDirectoryCreateFinishEventHandler?.Invoke(this, response.IsSuccess);
        }

        /// <summary>
        /// 文件名重命名
        /// </summary>
        /// <param name="srcFileName"></param>
        /// <param name="targetFileName"></param>
        public void RemoteFileRename(string srcFileName, string targetFileName)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_RENAME,
                                new FileReNamePack()
                                {
                                    SourceFileName = srcFileName,
                                    TargetName = targetFileName
                                });
        }

        /// <summary>
        /// 文件重命名完成
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_RENAME_FINISH)]
        private void ReNameFinishHandler(SessionProviderContext session)
        {
            var file = GetMessageEntity<FileReNameFinishPack>(session);
            this.OnFileNameRenameFinishEventHandler?.Invoke(this, file.SourceFileName, file.TargetName, file.IsSuccess);
        }

        /// <summary>
        /// 获取程目录
        /// </summary>
        /// <param name="path"></param>
        public void GetRemoteRootTreeItems(string path)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_TREE_DIR,
                    new FileGetTreeDirectoryPack()
                    {
                        TargetRoot = path
                    });
        }

        /// <summary>
        /// 处理远程目录信息
        /// </summary>
        /// <param name="session"></param>
        [PacketHandler(MessageHead.C_FILE_TREE_DIRS)]
        private void TreeFilesHandler(SessionProviderContext session)
        {
            var pack = GetMessageEntity<FileTreeDirFilePack>(session);
            this.OnFileTreeItemsEventHandler?.Invoke(this, pack.FileList);
        }

        /// <summary>
        /// 远程执行文件
        /// </summary>
        /// <param name="path"></param>
        public void RemoteExecuteFile(string path)
        {
            SendTo(CurrentSession, MessageHead.S_FILE_EXECUTE,
                new FileExcutePack()
                {
                    FilePath = path
                });

        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="remoteFileName">远程文件名</param>
        /// <returns></returns>
        public async Task DownloadFile(IFileStream fileStream, string remoteFileName)
        {
            //LogHelper.DebugWriteLog("begin download frist Data fileName:" + Path.GetFileName(localFileName));

            long position = fileStream.Length;
            reset:
            var responsed = await this.AwaitOpenDownloadData(remoteFileName, position);//首数据包，带文件状态信息及文件分块

            var status = 0;
            //返回null表示已断开连接
            if (responsed.IsNull())
            {
                var positionNull = await this.AwaitResetDownloadFile(fileStream);//等待重新连接
                if (positionNull.HasValue)
                {
                    position = positionNull.Value;
                    goto reset;
                }
                status = 0;
            }
            else if (responsed.Status == 1) //成功打开文件
            {
                await Task.Run(() => fileStream.Write(responsed.Data, 0, responsed.Data.Length));
                status = 1;
            }
            else
                status = responsed.Status;//文件访问失败

            this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.Begin, remoteFileName, position, responsed.FileSize);
            while (status == 1)
            {
                if (TransferTaskFlage == TransferTaskFlage.Abort)
                {
                    //停止传输
                    this.RemoteTaskStop();
                    break;
                }
                if (fileStream.Length >= responsed.FileSize)
                    break;//文件传输完成

                var data = await this.AwaitDownloadDataPack().ConfigureAwait(true);
                //LogHelper.DebugWriteLog("download data:" + (data == null ? "null" : data.Data.Length.ToString()));
                if (this.WhetherClose)
                    break;//传输中途关闭应用
                if (data.IsNull())
                {
                    var positionNull = await this.AwaitResetDownloadFile(fileStream).ConfigureAwait(true);
                    if (!positionNull.HasValue)
                        break;//session断开期间应用被关闭

                    position = positionNull.Value;
                    goto reset;
                }
                await Task.Run(() => fileStream.Write(data.Data, 0, data.Data.Length));

                this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.Transfering, remoteFileName, fileStream.Length, responsed.FileSize);
            }

            this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.End, remoteFileName, fileStream.Length, responsed.FileSize);

            fileStream.Close();
        }

        private async Task<long?> AwaitResetDownloadFile(IFileStream fileStream)
        {
            return await Task.Run(() =>
            {
                _sessionOfLinesEvent.WaitOne();//等待重连
                if (this.WhetherClose)
                    return null;

                long? position = fileStream.Length;
                return position;
            });
        }
        [PacketHandler(MessageHead.C_FILE_FRIST_DATA)]
        private void SetOpenEvent(SessionProviderContext session)
        {
            //LogHelper.DebugWriteLog("C_FILE_FRIST_DATA SetOpenEvent head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()) /*+ " fileName:" + session.CompletedBuffer.GetMessageEntity<FileFristDownloadDataPack>().fileName*/);
            var data = GetMessage(session);
            _workerStreamEvent.SetOneData(data);
        }
        private async Task<FileFristDownloadDataPack> AwaitOpenDownloadData(string remoteFileName, long position)
        {
            return await Task.Run(() =>
            {
                SendTo(CurrentSession, MessageHead.S_FILE_DOWNLOAD,
                new FileDownloadPack()
                {
                    FileName = remoteFileName,
                    Position = position
                });

                if (_isWorkSessionOfLines)//判断是否离线，再进入阻塞等待
                {
                    var data = _workerStreamEvent.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    //LogHelper.DebugWriteLog("AwaitFristDownloadData head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()) + " buffer lenght:" + data.Length);
                    return PacketSerializeHelper.DeserializePacket<FileFristDownloadDataPack>(data);
                }
                else
                    return null;
            });
        }

        [PacketHandler(MessageHead.C_FILE_DATA)]
        private void SetDataOneEvent(SessionProviderContext session)
        {
            //LogHelper.DebugWriteLog("SetDataOneEvent head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()));
            var data = GetMessage(session);
            _workerStreamEvent.SetOneData();
        }
        private async Task<FileDownloadDataPack> AwaitDownloadDataPack()
        {
            return await Task.Run(() =>
            {

                SendTo(CurrentSession, MessageHead.S_FILE_NEXT_DATA);
                if (_isWorkSessionOfLines)
                {
                    var data = _workerStreamEvent.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    //LogHelper.DebugWriteLog("AwaitDataPack head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()) + " buffer lenght:" + data.Length);
                    return PacketSerializeHelper.DeserializePacket<FileDownloadDataPack>(data);
                }
                else
                    return null;
            });
        }

        private void RemoteTaskStop()
        {
            SendTo(CurrentSession, MessageHead.S_FILE_STOP);//停止任务，通知远程关闭文件
        }

        /// <summary>
        /// 停止所有的传输任务
        /// </summary>
        public void StopTransferTask()
        {
            this.TransferTaskFlage = TransferTaskFlage.Abort;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileStream">读取文件流</param>
        /// <param name="remoteFileName">远程文件名</param>
        /// <param name="onSelectedFileTransferMode">选择覆盖模式</param>
        /// <returns></returns>
        public async Task UploadFile(
            IFileStream fileStream,
            string remoteFileName,
            Func<string, TransferMode> onSelectedFileTransferMode)
        {
            reset:

            LogHelper.DebugWriteLog("begin upload");
            var responsed = await this.AwaitOpenUploadFileStatus(remoteFileName);//获取远程文件状态
            if (responsed.IsNull())//返回null表示等待结果期间连接中断
            {
                var isReset = await this.AwaitResetUploadFile();
                if (isReset)
                    goto reset;
                else
                {
                    fileStream.Close();
                    return;
                }
            }

            long position = 0;
            int fileMode = 0;//0覆盖，1续传,2跳过
            if (responsed.Status == 1)
            {
                TransferMode transferMode = TransferMode.Continuingly;
                if (!_transferMode.HasValue)
                    transferMode = onSelectedFileTransferMode.Invoke(remoteFileName);
                else
                    transferMode = _transferMode.Value;

                switch (transferMode)
                {
                    case TransferMode.Replace:
                        fileMode = 0;
                        break;
                    case TransferMode.ReplaceAll:
                        fileMode = 0;
                        _transferMode = TransferMode.Replace;
                        break;
                    case TransferMode.Continuingly:
                        fileMode = 1;
                        position = responsed.Position;
                        break;
                    case TransferMode.ContinuinglyAll:
                        fileMode = 1;
                        position = responsed.Position;
                        _transferMode = TransferMode.Continuingly;
                        break;
                    case TransferMode.JumpOver:
                        CancelTransfer();
                        return;
                    case TransferMode.Cancel:
                        CancelTransfer();
                        _transferMode = transferMode;
                        return;
                    default:
                        break;
                }
            }
            else if (responsed.Status == 2)//文件访问失败
            {
                fileStream.Close();
                return;
            }

            fileStream.Position = position;
            var fileSize = fileStream.Length;

            this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.Begin, remoteFileName, position, fileSize);

            var data = await Task.Run(() => this.ReadFileStream(fileStream));

            SendTo(CurrentSession, MessageHead.S_FILE_FRIST_DATA,//上传首数据块，带文件选项及长度
                new FileFristUploadDataPack()
                {
                    FileMode = fileMode,
                    Position = position,
                    FileSize = fileStream.Length,
                    Data = data
                });

            while (true)
            {
                if (TransferTaskFlage == TransferTaskFlage.Abort)
                {
                    //停止
                    this.RemoteTaskStop();
                    fileStream.Close();
                    break;
                }

                if (fileStream.Position == fileStream.Length || fileMode == 2)
                {
                    //传输完成，或者跳过
                    fileStream.Close();
                    break;
                }
                position += data.Length;
                if (!await this.AwaitGetNextFileData())
                {
                    var isReset = await this.AwaitResetUploadFile();
                    if (isReset)
                        goto reset;
                    else
                    {
                        fileStream.Close();
                        break;
                    }
                }

                data = await Task.Run(() => this.ReadFileStream(fileStream));

                //底层通信库在正式发送数据包前会进行组包丶压缩等操作，由于文件数据块大，所处理耗时较长,此处使用线程以防止ui发生卡顿
                await Task.Run(() => SendTo(CurrentSession, MessageHead.S_FILE_DATA,
                            new FileUploadDataPack()
                            {
                                FileSize = fileStream.Length,
                                Data = data
                            }));

                this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.Transfering, remoteFileName, position, fileSize);
            }
            this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.End, remoteFileName, position, fileSize);

            //取消传输
            void CancelTransfer()
            {
                fileMode = 2;
                fileStream.Close();
                this.RemoteTaskStop();
                this.OnFileTransferProgressEventHandler?.Invoke(this, FileTransferFlag.End, remoteFileName, 0, 0);
            }
        }

        [PacketHandler(MessageHead.C_FILE_NEXT_DATA)]
        private void GetNextDataHandler(SessionProviderContext session)
            => _workerStreamEvent.SetOneData();

        private async Task<bool> AwaitGetNextFileData()
        {
            return await Task.Run(() =>
            {
                if (_isWorkSessionOfLines)
                    _workerStreamEvent.AwaitOneData();
                return _isWorkSessionOfLines;
            });
        }

        //重复使用缓冲区，减少内存碎片
        byte[] _fileBuffer = new byte[FILE_BUFFER_SIZE];
        private byte[] ReadFileStream(IFileStream fileStream)
        {
            int lenght = fileStream.Read(_fileBuffer, 0, _fileBuffer.Length);
            if (lenght == _fileBuffer.Length)
                return _fileBuffer;
            else
            {
                byte[] buf = new byte[lenght];//当数据小于缓冲区
                Array.Copy(_fileBuffer, buf, lenght);
                return buf;
            }
        }

        [PacketHandler(MessageHead.C_FILE_OPEN_STATUS)]
        private void SetUploadFileStatus(SessionProviderContext session)
        {
            //LogHelper.DebugWriteLog("SetUploadFileStatus head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()));
            _workerStreamEvent.SetOneData(GetMessage(session));
        }
        private async Task<FileUploadFileStatus> AwaitOpenUploadFileStatus(string remoteFileName)
        {
            return await Task.Run(() =>
            {
                SendTo(CurrentSession, MessageHead.S_FILE_UPLOAD,
                    new FileUploadPack()
                    {
                        FileName = remoteFileName
                    });
                //LogHelper.DebugWriteLog("get status");
                if (_isWorkSessionOfLines)
                {
                    var data = _workerStreamEvent.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    //LogHelper.DebugWriteLog("AwaitUploadFileStatus head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()));
                    return PacketSerializeHelper.DeserializePacket<FileUploadFileStatus>(data);
                }
                else
                    return null;
            });
        }

        private async Task<bool> AwaitResetUploadFile()
        {
            return await Task.Run(() =>
            {
                _sessionOfLinesEvent.WaitOne();//等待重连
                if (this.WhetherClose)
                    return false;
                else
                    return true;
            });
        }

        public async Task DownloadDirectory(
            string remotedirectory,
            Func<string, IFileStream> onCreateFileStream,
            Action<string> onCreateDicectroy)
        {
            reset:
            this._filesQueue.Clear();
            TransferTaskFlage = TransferTaskFlage.Allow;
            var result = await this.AwaitGetDirectoryFiles(remotedirectory);
            if (!result)
            {
                var isReset = await this.AwaitResetDownloadDirectory();
                if (isReset)
                    goto reset;
                else
                    return;
            }

            while (true)
            {
                if (_filesQueue.Count <= 0)
                    break;//文件夹所有文件传输完成

                if (TransferTaskFlage == TransferTaskFlage.Abort)
                    break;//停止任务

                var file = _filesQueue.Dequeue();
                if (file.Type == DirectoryFileType.File)
                {
                    var targetFileName = file.FileName;
                    var localFileName = file.FileName.Substring(remotedirectory.LastIndexOf("\\") + 1);
                    var fileStream = onCreateFileStream?.Invoke(localFileName);
                    if (fileStream.IsNull())
                        continue;
                    if (_transferMode == TransferMode.Cancel || TransferTaskFlage == TransferTaskFlage.Abort)
                        break;
                    await this.DownloadFile(fileStream, targetFileName);
                }
                else
                {
                    var directory = file.FileName.Substring(remotedirectory.LastIndexOf("\\") + 1);
                    onCreateDicectroy?.Invoke(directory);
                }

            }
            this._filesQueue.Clear();
        }
        [PacketHandler(MessageHead.C_FILE_DIR_FILES)]
        private void SetFilesTriggerEvent(SessionProviderContext session)
        {
            foreach (var file in GetMessageEntity<FileDirectoryFilesPack>(session).Files)
                _filesQueue.Enqueue(file);
            _filesTriggerEvent.Set();
        }

        private async Task<bool> AwaitGetDirectoryFiles(string remotedirectory)
        {
            return await Task.Run(() =>
            {
                SendTo(CurrentSession, MessageHead.S_FILE_GETDIR_FILES,
                    new FileDirectoryGetFilesPack()
                    {
                        DirectoryPath = remotedirectory
                    });
                if (this._isWorkSessionOfLines)
                {
                    _filesTriggerEvent.Reset();
                    _filesTriggerEvent.WaitOne();
                }

                return this._isWorkSessionOfLines;
            });
        }

        private async Task<bool> AwaitResetDownloadDirectory()
        {
            return await Task.Run(() =>
            {
                _sessionOfLinesEvent.WaitOne();//等待重连
                if (this.WhetherClose)
                    return false;
                else
                    return true;
            });
        }

        public override void SessionClosed(SessionProviderContext session)
        {
            this._isWorkSessionOfLines = false;
            this._filesTriggerEvent.Set();//释放
            this._sessionOfLinesEvent.Reset();//阻塞等待重连
            this._workerStreamEvent.SetOneData();//如果有正在等待数据响应的，则先释放信号，进入重置方法
            LogHelper.DebugWriteLog("close eventSet");
            if (this.WhetherClose)//如果应用已关闭,则释放退出
                this._sessionOfLinesEvent.Set();

            base.SessionClosed(session);
        }

        public override void ContinueTask(SessionProviderContext session)
        {
            this._workerStreamEvent.Reset();
            this._isWorkSessionOfLines = true;
            this._sessionOfLinesEvent.Set();

            base.ContinueTask(session);
        }
    }
}
