using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.FileManager;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.Enums;
using SiMay.RemoteMonitor.MainApplication;
using SiMay.RemoteMonitor.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("文件管理")]
    [AppResourceName("FileManager")]
    [Application(typeof(RemoteFileAdapterHandler), "FileManagerJob", 10)]
    public partial class FileApplication : Form, IApplication
    {
        private const Int32 IDM_DIR_DESKTOP = 1000;
        private const Int32 IDM_DIR_DOC = 1001;
        private const Int32 IDM_DIR_MUSIC = 1002;
        private const Int32 IDM_DIR_VIDEO = 1003;
        private const Int32 IDM_DIR_PIC = 1004;
        private const Int32 IDM_DIR_HOME = 1005;

        [ApplicationAdapterHandler]
        public RemoteFileAdapterHandler RemoteFileAdapterHandler { get; set; }

        //private const string Log_FileName = "FileManagerLog.log";
        private const int FILE_BUFFER_SIZE = 1024 * 512;

        private string _title = "//远程文件管理 #Name#";

        /// <summary>
        /// 任务状态，0停止，1开始
        /// </summary>
        private int _taskStatus = 1;
        private volatile bool _isWorkSessionOfLines = true; //session是否在线状态
        private string[] _copyFileNames = null;
        private AwaitAutoResetEvent _event = new AwaitAutoResetEvent(false);
        private ManualResetEvent _sessionOfLinesEvent = new ManualResetEvent(true);
        private ManualResetEvent _filesTrigger = new ManualResetEvent(false);
        private Queue<DirectoryFileItem> _filesQueue = new Queue<DirectoryFileItem>();

        private TreeView _remoteDirectoryTreeView;
        private Button _closeTreeBtn;
        private DateTime _startTime;//任务开始时间
        private ImageList _imgList = new ImageList();
        private Hashtable _icoHash = new Hashtable();
        private FileIconUtil _iconUtil = new FileIconUtil();
        private dynamic _adapter;
        private FileTransferModeForm.TransferMode? _transferMode = null;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public FileApplication()
        {
            _adapter = new object();
            //_adapter = adapter;
            //adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            //_title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();
        //private void Adapter_OnSessionNotifyPro(SessionHandler session, SessionNotifyType notify)
        //{
        //    switch (notify)
        //    {
        //        case SessionNotifyType.Message:
        //            if (_adapter.WindowClosed)
        //                return;
        //            var result = this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        //            if (!result)
        //                LogHelper.DebugWriteLog("model invoke:" + (short)session.CompletedBuffer.GetMessageHead() + " result:" + result.ToString());
        //            break;
        //        case SessionNotifyType.OnReceive:
        //            break;
        //        case SessionNotifyType.ContinueTask:
        //            this.ContinueTask();
        //            break;
        //        case SessionNotifyType.SessionClosed:
        //            this.SessionClosed();
        //            break;
        //        case SessionNotifyType.WindowShow:
        //            this.Show();
        //            break;
        //        case SessionNotifyType.WindowClose:
        //            _adapter.WindowClosed = true;
        //            this.Close();
        //            break;
        //        default:
        //            break;
        //    }
        //}

        private void FileManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;

            this._closeTreeBtn = new Button();
            this._closeTreeBtn.Click += _closeTreeBtn_Click;
            this._closeTreeBtn.Hide();
            this._closeTreeBtn.Text = "收起";
            this._closeTreeBtn.Height = 25;
            this._closeTreeBtn.Width = 100;
            this._remoteDirectoryTreeView = new TreeView();
            this._remoteDirectoryTreeView.ImageList = _imgList;
            this._remoteDirectoryTreeView.ContextMenuStrip = this.treeContext;
            this._remoteDirectoryTreeView.DoubleClick += remoteDirectoryTreeView_DoubleClick;
            this._remoteDirectoryTreeView.Hide();
            this.Controls.Add(_remoteDirectoryTreeView);
            this.Controls.Add(_closeTreeBtn);
            this.Initialize();
        }



        private void Initialize()
        {
            string downPath = Path.Combine(Environment.CurrentDirectory, "download");

            if (!Directory.Exists(downPath))
                Directory.CreateDirectory(downPath);

            this.txtSavePath.Text = downPath;
            this.fileList.SmallImageList = _imgList;
            this.fileList.LargeImageList = _imgList;

            _adapter.SendAsyncMessage(MessageHead.S_FILE_TREE_DIR, new FileGetTreeDirectoryPack() { TargetRoot = "" });//获取根目录
            _adapter.SendAsyncMessage(MessageHead.S_FILE_GET_DRIVES);

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            int index = 7;
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_DESKTOP, "我的桌面");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_DOC, "我的文档");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_PIC, "我的图片");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_VIDEO, "我的视频");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_MUSIC, "我的音乐");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIR_HOME, "所有磁盘");
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_DIR_DESKTOP:
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_REDIRION, new FileRedirectionPath()
                        {
                            SpecialFolder = Environment.SpecialFolder.DesktopDirectory
                        });
                        break;
                    case IDM_DIR_DOC:
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_REDIRION, new FileRedirectionPath()
                        {
                            SpecialFolder = Environment.SpecialFolder.MyDocuments
                        });
                        break;
                    case IDM_DIR_MUSIC:
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_REDIRION, new FileRedirectionPath()
                        {
                            SpecialFolder = Environment.SpecialFolder.MyMusic
                        });
                        break;
                    case IDM_DIR_PIC:
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_REDIRION, new FileRedirectionPath()
                        {
                            SpecialFolder = Environment.SpecialFolder.MyPictures
                        });
                        break;
                    case IDM_DIR_VIDEO:
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_REDIRION, new FileRedirectionPath()
                        {
                            SpecialFolder = Environment.SpecialFolder.MyVideos
                        });
                        break;
                    case IDM_DIR_HOME:
                        _adapter.SendAsyncMessage(
                            MessageHead.S_FILE_GET_DRIVES);
                        break;
                }
            }
            base.WndProc(ref m);
        }

        public void SessionClosed()
        {
            this.Text = _title + " [" + _adapter.TipText + "]";
            this._isWorkSessionOfLines = false;
            this._filesTrigger.Set();//释放
            this._sessionOfLinesEvent.Reset();//阻塞等待重连
            this._event.SetOneData();//如果有正在等待数据响应的，则先释放信号，进入重置方法
            LogHelper.DebugWriteLog("close eventSet");
            if (_adapter.WindowClosed)//如果窗口已关闭,则释放退出
                this._sessionOfLinesEvent.Set();
        }

        public void ContinueTask()
        {
            this.Text = _title;
            this._event.Reset();
            this._isWorkSessionOfLines = true;
            this._sessionOfLinesEvent.Set();
        }

        [PacketHandler(MessageHead.C_FILE_PASTER_FINISH)]
        public void RefreshDirectory(SessionHandler session)
        {
            this.GetFiles(txtRemotedirectory.Text);
        }

        [PacketHandler(MessageHead.C_FILE_ERROR_INFO)]
        public void WriteExceptionLog(SessionHandler session)
        {
            var log = session.CompletedBuffer.GetMessageEntity<FileExceptionPack>();
            var sb = new StringBuilder();
            sb.AppendLine("remoteService Exception:");
            sb.AppendLine("occurrence time:" + log.OccurredTime.ToString());
            sb.AppendLine("TipMessage:" + log.TipMessage);
            sb.AppendLine("ExceptionMessage:" + log.ExceptionMessage);
            sb.AppendLine("StackTrace:" + log.StackTrace);
            LogHelper.WriteErrorByCurrentMethod(sb.ToString());
        }

        [PacketHandler(MessageHead.C_FILE_FILE_LIST)]
        public void FixedRemoteFileList(SessionHandler session)
        {
            var fileLst = session.CompletedBuffer.GetMessageEntity<FileListItemsPack>();
            this.fileList.Items.Clear();

            if (!fileLst.IsSccessed)
                MessageBoxHelper.ShowBoxExclamation(fileLst.Message);

            this.txtRemotedirectory.Text = fileLst.Path;

            foreach (var file in fileLst.FileList)
                this.AddItem(file);

            this.transferCaption.Text = "装载目录" + txtRemotedirectory.Text + "完成,共 " + fileList.Items.Count.ToString() + " 个对象";
        }

        private void AddItem(FileItem file)
        {
            switch (file.FileType)
            {
                case FileType.File:
                    string extension = Path.GetExtension(file.FileName);
                    if (extension == "")
                        extension = ".kksksxx";

                    this.fileList.Items.Add(new FileListViewItem(
                        file.FileName,
                        file.FileSize,
                        file.UsingSize,
                        file.FreeSize,
                        FileItemType.File,
                        file.LastAccessTime,
                        IcoIndex(extension, true)));

                    break;
                case FileType.Directory:
                    this.fileList.Items.Add(new FileListViewItem(
                        file.FileName,
                        file.FileSize,
                        file.UsingSize,
                        file.FreeSize,
                        FileItemType.Directory,
                        file.LastAccessTime,
                        IcoIndex("DIR", false)));
                    break;
                case FileType.Disk:
                    this.fileList.Items.Add(new FileListViewItem(
                        file.FileName,
                        file.FileSize,
                        file.UsingSize,
                        file.FreeSize,
                        FileItemType.Disk,
                        file.LastAccessTime,
                        IcoIndex("", true)));
                    break;
            }
        }

        private int IcoIndex(string extension, bool isFile)
        {
            if (this._icoHash.ContainsKey(extension))
            {
                return (int)_icoHash[extension];
            }
            else
            {
                if (isFile)
                {
                    _imgList.Images.Add(this._iconUtil.GetIcon(extension, false).ToBitmap());
                    _icoHash.Add(extension, _imgList.Images.Count - 1);
                    return (int)_icoHash[extension];
                }
                else
                {
                    _imgList.Images.Add(_iconUtil.GetDirectoryIcon(false).ToBitmap());
                    _icoHash.Add(extension, _imgList.Images.Count - 1);
                    return (int)_icoHash[extension];
                }
            }
        }

        private void FileManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._handlerBinder.Dispose();
            this._adapter.WindowClosed = true;
            this._isWorkSessionOfLines = false;
            this._event.SetOneData();
            this._sessionOfLinesEvent.Set();
            this._filesQueue.Clear();
            this._adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OnSelectRemoteOpenFile();
        }

        private void OnSelectRemoteOpenFile()
        {
            if (this.fileList.SelectedItems.Count != 0)
            {
                var file = this.fileList.Items[fileList.SelectedItems[0].Index] as FileListViewItem;
                if (file.FileType == FileItemType.File)
                {
                    if (file.FileSize > 1024 * 512)
                    {
                        MessageBoxHelper.ShowBoxExclamation("仅支持打开512KB以下的文件!");
                        return;
                    }
                    if (MessageBox.Show("系统仅支持记事本方式打开 " + file.FileName + " ,确定继续吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                        return;
                    _adapter.SendAsyncMessage(MessageHead.S_FILE_OPEN_TEXT,
                                    new FileOpenTextPack()
                                    {
                                        FileName = Path.Combine(txtRemotedirectory.Text, file.FileName)
                                    });
                }
                else
                {
                    var currentPath = txtRemotedirectory.Text;
                    if (currentPath.IsNullOrEmpty())
                        currentPath = file.FileName;
                    else
                        currentPath = Path.Combine(currentPath, file.FileName);

                    txtRemotedirectory.Text = currentPath;
                    this.GetFiles(txtRemotedirectory.Text);
                }
            }
        }
        [PacketHandler(MessageHead.C_FILE_TEXT)]
        public void OpenRemoteText(SessionHandler session)
        {
            var text = session.CompletedBuffer.GetMessageEntity<FileTextPack>();
            if (!text.IsSuccess)
            {
                MessageBoxHelper.ShowBoxExclamation("远程文件打开失败!");
                return;
            }
            var tmp = Path.Combine(Environment.CurrentDirectory, "tmp");
            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            string randomName = Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(Path.Combine(tmp, randomName), text.Text);

            Process.Start(Path.Combine(tmp, randomName));
        }

        private void GetFiles(string path)
        {
            _adapter.SendAsyncMessage(MessageHead.S_FILE_GET_FILES,
                new FileListPack()
                {
                    FilePath = path
                });
        }
        private void RemoteExecuteFile(string path)
        {
            _adapter.SendAsyncMessage(MessageHead.S_FILE_EXECUTE,
                new FileExcutePack()
                {
                    FilePath = path
                });

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!txtRemotedirectory.Text.IsNullOrEmpty())
                this.GetFiles(txtRemotedirectory.Text);
            else
            {
                _adapter.SendAsyncMessage(
                    MessageHead.S_FILE_GET_DRIVES);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string newPath = Path.GetDirectoryName(txtRemotedirectory.Text);
                txtRemotedirectory.Text = newPath;
                if (newPath.IsNullOrEmpty())
                {
                    _adapter.SendAsyncMessage(
                            MessageHead.S_FILE_GET_DRIVES);
                }
                else
                    this.GetFiles(newPath);
            }
            catch (Exception)
            {
                _adapter.SendAsyncMessage(
                        MessageHead.S_FILE_GET_DRIVES);
            }
        }

        private void m_files_DoubleClick(object sender, EventArgs e)
        {
            this.OnSelectRemoteOpenFile();
        }

        private FileListViewItem[] GetSelectFiles()
        {
            var returnValues = new List<FileListViewItem>();
            if (fileList.SelectedItems.Count != 0)
            {
                var selectItems = fileList.SelectedItems;
                foreach (ListViewItem item in selectItems)
                    item.Checked = true;
                for (int i = 0; i < fileList.Items.Count; i++)
                {
                    if (fileList.Items[i].Checked)
                    {
                        var item = fileList.Items[i] as FileListViewItem;
                        returnValues.Add(item);
                        item.Checked = false;
                    }
                }
            }
            return returnValues.ToArray();
        }
        private void 删除文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var files = this.GetSelectFiles();
            if (files.Any(c => c.FileType == FileItemType.Disk))
            {
                MessageBoxHelper.ShowBoxExclamation("根目录不允许删除!");
                return;
            }

            if (MessageBox.Show("确定要删除这些文件吗?", "提示",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation) != DialogResult.OK)
                return;

            _adapter.SendAsyncMessage(
                MessageHead.S_FILE_DELETE,
                new FileDeletePack()
                {
                    FileNames = files.Select(c => Path.Combine(txtRemotedirectory.Text, c.FileName)).ToArray()
                });

        }
        [PacketHandler(MessageHead.C_FILE_DELETE_FINISH)]
        public void DeleteFinishHandler(SessionHandler session)
        {
            var files = session.CompletedBuffer.GetMessageEntity<FileDeleteFinishPack>();
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                FileListViewItem item = fileList.Items[i] as FileListViewItem;
                foreach (var file in files.DeleteFileNames)
                {
                    if (item.FileName == Path.GetFileName(file))
                    {
                        item.Remove();
                        i--;
                    }
                }
            }
        }
        private void 新建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtRemotedirectory.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxExclamation("根目录不能创建文件夹!");
                return;
            }

            EnterForm dlg = new EnterForm();
            dlg.Caption = "请输入文件夹名称!";
            if (dlg.ShowDialog() == DialogResult.OK && !dlg.Value.IsNullOrEmpty())
            {
                foreach (FileListViewItem item in fileList.Items)
                {
                    if (item.FileName.ToLower() == dlg.Value.ToLower())
                    {
                        MessageBoxHelper.ShowBoxExclamation("已与现有文件重名!");
                        return;
                    }
                }
                if (FileHelper.HasIllegalCharacters(dlg.Value))
                {
                    var targetPath = Path.Combine(txtRemotedirectory.Text, dlg.Value);
                    if (!this.RemoteCreateDirectory(targetPath))
                    {
                        MessageBoxHelper.ShowBoxExclamation("文件夹创建失败,目标路径过长!");
                    }
                }
                else
                {
                    MessageBoxHelper.ShowBoxError("文件名不能包含 | / \\ * ? \" < > : 等特殊符号!");
                }
            }
        }

        private bool RemoteCreateDirectory(string path, bool noCallback = false)
        {
            if (FileHelper.VerifyLongPath(path))
            {
                LogHelper.WriteErrorByCurrentMethod("Create Directory 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。remote:{0}".FormatTo(path));
                return false;
            }

            _adapter.SendAsyncMessage(MessageHead.S_FILE_CREATE_DIR,
                new FileCreateDirectoryPack()
                {
                    DirectoryName = path,
                    NoCallBack = noCallback
                });

            return true;
        }

        [PacketHandler(MessageHead.C_FILE_CREATEF_DIR_FNISH)]
        public void DirectoryCreateFinishHandler(SessionHandler session)
        {
            var response = session.CompletedBuffer.GetMessageEntity<FileCreateDirectoryFinishPack>();
            if (response.IsSuccess)
                this.GetFiles(txtRemotedirectory.Text);
            else
                MessageBoxHelper.ShowBoxError("文件夹创建失败!");
        }
        private void 选择全部ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in fileList.Items)
                item.Checked = true;
        }

        private void 取消选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in fileList.Items)
                item.Checked = false;
        }



        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var files = this.GetSelectFiles();
            if (files.Any())
            {
                var file = files.FirstOrDefault();
                if (file.FileType == FileItemType.Disk)
                {
                    MessageBoxHelper.ShowBoxError("磁盘不能作为重命名的对象!");
                    return;
                }
                EnterForm dlg = new EnterForm();
                dlg.Caption = "请为 " + file.FileName + " 命名新的名称!";
                dlg.Value = file.FileName;
                if (dlg.Value != "" && dlg.ShowDialog() == DialogResult.OK)
                {
                    if (FileHelper.HasIllegalCharacters(dlg.Value))
                    {
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_RENAME,
                            new FileReNamePack()
                            {
                                SourceFileName = Path.Combine(txtRemotedirectory.Text, file.FileName),
                                TargetName = Path.Combine(txtRemotedirectory.Text, dlg.Value)
                            });
                    }
                    else
                    {
                        MessageBoxHelper.ShowBoxError("文件名不能包含 | / \\ * ? \" < > : 等特殊符号!");
                    }
                }

            }
        }
        [PacketHandler(MessageHead.C_FILE_RENAME_FINISH)]
        public void ReNameFinishHandler(SessionHandler session)
        {
            var file = session.CompletedBuffer.GetMessageEntity<FileReNameFinishPack>();
            if (file.IsSuccess)
            {
                for (int i = 0; i < fileList.Items.Count; i++)
                {
                    FileListViewItem item = fileList.Items[i] as FileListViewItem;
                    if (item.FileName == Path.GetFileName(file.SourceFileName))
                    {
                        item.Text = Path.GetFileName(file.TargetName);
                        item.FileName = Path.GetFileName(file.TargetName);
                        break;
                    }

                }
            }
            else
                MessageBoxHelper.ShowBoxError("文件重命名失败!");
        }
        private void 复制文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var files = this.GetSelectFiles();
            if (files.Any(c => c.FileType == FileItemType.Disk))
            {
                MessageBoxHelper.ShowBoxError("磁盘对象不支持复制!");
                return;
            }
            if (files.Length > 0)
                _copyFileNames = files.Select(c => Path.Combine(txtRemotedirectory.Text, c.FileName)).ToArray();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_copyFileNames.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxError("请选择要复制的文件!");
                return;
            }
            if (txtRemotedirectory.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxError("当前路径不能粘贴!");
                return;
            }

            _adapter.SendAsyncMessage(MessageHead.S_FILE_FILE_PASTER,
                new FileCopyPack()
                {
                    TargetDirectoryPath = txtRemotedirectory.Text,
                    FileNames = _copyFileNames
                });
            transferCaption.Text = "已向远程发出粘贴请求,请等待完成反馈!";
        }
        [PacketHandler(MessageHead.C_FILE_COPY_FINISH)]
        public void CopyFileFinishHandler(SessionHandler session)
        {
            var fails = session.CompletedBuffer.GetMessageEntity<FileCopyFinishPack>();

            this.GetFiles(txtRemotedirectory.Text);
            if (fails.ExceptionFileNames.Length > 0)
                MessageBoxHelper.ShowBoxExclamation($"文件复制完成，但有部分文件复制操作异常:{string.Join(", ", fails.ExceptionFileNames)},请检查!");
            else
                MessageBoxHelper.ShowBoxExclamation("文件复制完成!");
        }
        private void downloadMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认下载该文件吗", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                this.StartDownload(txtRemotedirectory.Text, txtSavePath.Text);
        }

        private async void StartDownload(string targetRoot, string localRoot)
        {
            this.downloadMenuItem.Enabled = false;
            this.uploadMenuItem.Enabled = false;
            this.downloadAsToolStripMenuItem.Enabled = false;
            this._transferMode = null;
            this._taskStatus = 1;
            this._startTime = DateTime.Now;
            var root = targetRoot;
            var savePath = localRoot;
            foreach (var fileItem in this.GetSelectFiles())
            {
                if (fileItem.FileType == FileItemType.File)
                {
                    string remoteFileName = Path.Combine(root, fileItem.FileName);
                    string localFileName = Path.Combine(savePath, fileItem.FileName);
                    await this.DownloadFile(remoteFileName, localFileName);
                }
                else if (fileItem.FileType == FileItemType.Directory)
                    await this.DownloadDirectory(Path.Combine(root, fileItem.FileName), savePath);
                else
                {
                    MessageBoxHelper.ShowBoxExclamation("当前选项中包含了暂未支持传输的部分!");
                    break;
                }
                if (_transferMode == FileTransferModeForm.TransferMode.Cancel)
                    break;//取消传输

                if (_adapter.WindowClosed)
                    break;
            }
            this.downloadMenuItem.Enabled = true;
            this.uploadMenuItem.Enabled = true;
            this.downloadAsToolStripMenuItem.Enabled = true;
        }
        private async Task DownloadFile(string remoteFileName, string localFileName)
        {
            if (FileHelper.VerifyLongPath(localFileName) || FileHelper.VerifyLongPath(remoteFileName))
            {
                LogHelper.WriteErrorByCurrentMethod("DownloadFile 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。local:{0},remote:{1}".FormatTo(localFileName, remoteFileName));
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(localFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(localFileName));

            long position = 0;
            if (File.Exists(localFileName))
            {
                FileTransferModeForm.TransferMode transferMode = FileTransferModeForm.TransferMode.Continuingly;
                if (!_transferMode.HasValue)
                {
                    FileTransferModeForm dlg = new FileTransferModeForm();
                    dlg.TipMessage = "此文件夹已包含一个名为 " + Path.GetFileName(localFileName) + " 的文件";
                    dlg.ShowDialog();
                    transferMode = dlg.TransferModeResult;
                }
                else
                    transferMode = _transferMode.Value;

                switch (transferMode)
                {
                    case FileTransferModeForm.TransferMode.Replace:
                        File.Delete(localFileName);
                        break;
                    case FileTransferModeForm.TransferMode.ReplaceAll:
                        File.Delete(localFileName);
                        _transferMode = FileTransferModeForm.TransferMode.Replace;
                        break;
                    case FileTransferModeForm.TransferMode.Continuingly:
                        position = new FileInfo(localFileName).Length;
                        break;
                    case FileTransferModeForm.TransferMode.ContinuinglyAll:
                        position = new FileInfo(localFileName).Length;
                        _transferMode = FileTransferModeForm.TransferMode.Continuingly;
                        break;
                    case FileTransferModeForm.TransferMode.JumpOver:
                        return;//跳过本次
                    case FileTransferModeForm.TransferMode.Cancel:
                        _transferMode = transferMode;
                        return;
                    default:
                        break;
                }
            }
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(
                    localFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                fileStream.Position = position;//从断点写入
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod("打开本地文件失败," + ex.Message);
                return;
            }

            reset:
            _adapter.SendAsyncMessage(MessageHead.S_FILE_DOWNLOAD,
                new FileDownloadPack()
                {
                    FileName = remoteFileName,
                    Position = position
                });
            LogHelper.DebugWriteLog("begin download frist Data fileName:" + Path.GetFileName(localFileName));
            var responsed = await this.AwaitFristDownloadData();//首数据包，带文件状态信息及文件分块

            var status = 0;
            //返回null表示已断开连接
            if (responsed == null)
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
                await fileStream
                    .WriteAsync(responsed.Data, 0, responsed.Data.Length)
                    .ConfigureAwait(true);

                status = 1;
            }
            else
                status = responsed.Status;//文件访问失败

            this.TransferNotify(FileTransferNotify.Begin, remoteFileName, position, responsed.FileSize);
            while (status == 1)
            {
                if (_taskStatus == 0)
                {
                    //停止传输
                    this.RemoteTaskStop();
                    break;
                }
                if (fileStream.Length >= responsed.FileSize)
                    break;//文件传输完成

                var data = await this.AwaitDataPack().ConfigureAwait(true);
                //LogHelper.DebugWriteLog("download data:" + (data == null ? "null" : data.Data.Length.ToString()));
                if (_adapter.WindowClosed)
                    break;//传输中途关闭窗口
                if (data == null)
                {
                    var positionNull = await this.AwaitResetDownloadFile(fileStream).ConfigureAwait(true);
                    if (!positionNull.HasValue)
                        break;//session断开期间窗口被关闭

                    position = positionNull.Value;
                    goto reset;
                }
                await fileStream
                    .WriteAsync(data.Data, 0, data.Data.Length)
                    .ConfigureAwait(true);

                this.TransferNotify(FileTransferNotify.Transfering, remoteFileName, fileStream.Length, responsed.FileSize);
            }

            this.TransferNotify(FileTransferNotify.End, remoteFileName, fileStream.Length, responsed.FileSize);

            await fileStream
                .FlushAsync()
                .ConfigureAwait(false);

            fileStream.Close();

        }

        private async Task<long?> AwaitResetDownloadFile(FileStream fileStream)
        {
            return await Task.Run(() =>
            {
                _sessionOfLinesEvent.WaitOne();//等待重连
                if (_adapter.WindowClosed)
                    return null;

                long? position = fileStream.Length;
                return position;
            });
        }
        [PacketHandler(MessageHead.C_FILE_FRIST_DATA)]
        public void SetOpenEvent(SessionHandler session)
        {
            LogHelper.DebugWriteLog("C_FILE_FRIST_DATA SetOpenEvent head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()) /*+ " fileName:" + session.CompletedBuffer.GetMessageEntity<FileFristDownloadDataPack>().fileName*/);
            _event.SetOneData(session.CompletedBuffer);
        }
        private async Task<FileFristDownloadDataPack> AwaitFristDownloadData()
        {
            return await Task.Run(() =>
            {

                if (_isWorkSessionOfLines)//判断是否离线，再进入阻塞等待
                {
                    var data = _event.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    LogHelper.DebugWriteLog("AwaitFristDownloadData head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()) + " buffer lenght:" + data.Length);
                    return data.GetMessageEntity<FileFristDownloadDataPack>();
                }
                else
                    return null;
            });
        }

        [PacketHandler(MessageHead.C_FILE_DATA)]
        public void SetDataOneEvent(SessionHandler session)
        {
            LogHelper.DebugWriteLog("SetDataOneEvent head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()));
            _event.SetOneData(session.CompletedBuffer);
        }
        private async Task<FileDownloadDataPack> AwaitDataPack()
        {
            return await Task.Run(() =>
            {

                _adapter.SendAsyncMessage(MessageHead.S_FILE_NEXT_DATA);
                if (_isWorkSessionOfLines)
                {
                    var data = _event.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    LogHelper.DebugWriteLog("AwaitDataPack head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()) + " buffer lenght:" + data.Length);
                    return data.GetMessageEntity<FileDownloadDataPack>();
                }
                else
                    return null;
            });
        }


        private void RemoteTaskStop()
        {
            _adapter.SendAsyncMessage(MessageHead.S_FILE_STOP);//停止任务，通知远程关闭文件
        }

        private async Task UploadFile(string localFileName, string remoteFileName)
        {
            if (FileHelper.VerifyLongPath(localFileName) || FileHelper.VerifyLongPath(remoteFileName))
            {
                LogHelper.WriteErrorByCurrentMethod("UploadFile 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。local:{0},remote:{1}".FormatTo(localFileName, remoteFileName));
                return;
            }

            FileStream fileStream;
            try
            {
                fileStream = new FileStream(
                    localFileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
                return;
            }

            reset:
            _adapter.SendAsyncMessage(MessageHead.S_FILE_UPLOAD,
                new FileUploadPack()
                {
                    FileName = remoteFileName
                });
            LogHelper.DebugWriteLog("begin upload");
            var responsed = await this.AwaitUploadFileStatus();//获取远程文件状态
            if (responsed == null)//返回null表示等待结果期间连接中断
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
                FileTransferModeForm.TransferMode transferMode = FileTransferModeForm.TransferMode.Continuingly;
                if (!_transferMode.HasValue)
                {
                    FileTransferModeForm dlg = new FileTransferModeForm();
                    dlg.TipMessage = "此文件夹已包含一个名为 " + Path.GetFileName(remoteFileName) + " 的文件";
                    dlg.ShowDialog();
                    transferMode = dlg.TransferModeResult;
                }
                else
                    transferMode = _transferMode.Value;

                switch (transferMode)
                {
                    case FileTransferModeForm.TransferMode.Replace:
                        fileMode = 0;
                        break;
                    case FileTransferModeForm.TransferMode.ReplaceAll:
                        fileMode = 0;
                        _transferMode = FileTransferModeForm.TransferMode.Replace;
                        break;
                    case FileTransferModeForm.TransferMode.Continuingly:
                        fileMode = 1;
                        position = responsed.Position;
                        break;
                    case FileTransferModeForm.TransferMode.ContinuinglyAll:
                        fileMode = 1;
                        position = responsed.Position;
                        _transferMode = FileTransferModeForm.TransferMode.Continuingly;
                        break;
                    case FileTransferModeForm.TransferMode.JumpOver:
                        CancelTransfer();
                        return;
                    case FileTransferModeForm.TransferMode.Cancel:
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

            this.TransferNotify(FileTransferNotify.Begin, remoteFileName, position, fileSize);

            var data = await this.ReadAsync(fileStream);
            _adapter.SendAsyncMessage(MessageHead.S_FILE_FRIST_DATA,//上传首数据块，带文件选项及长度
                new FileFristUploadDataPack()
                {
                    FileMode = fileMode,
                    Position = position,
                    FileSize = fileStream.Length,
                    Data = data
                });

            while (true)
            {
                if (_taskStatus == 0)
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

                data = await this.ReadAsync(fileStream);

                //底层通信库在正式发送数据包前会进行组包丶压缩等操作，由于文件数据块大，所处理耗时较长,此处使用线程以防止ui发生卡顿
                await Task.Run(() =>
                        _adapter.SendAsyncMessage(MessageHead.S_FILE_DATA,
                            new FileUploadDataPack()
                            {
                                FileSize = fileStream.Length,
                                Data = data
                            }));

                this.TransferNotify(FileTransferNotify.Transfering, remoteFileName, position, fileSize);
            }
            this.TransferNotify(FileTransferNotify.End, remoteFileName, position, fileSize);

            //取消传输
            void CancelTransfer()
            {
                fileMode = 2;
                fileStream.Close();
                this.RemoteTaskStop();
                this.TransferNotify(FileTransferNotify.End, remoteFileName, 0, 0);
            }
        }

        [PacketHandler(MessageHead.C_FILE_NEXT_DATA)]
        public void GetNextDataHandler(SessionHandler session)
            => _event.SetOneData();

        private async Task<bool> AwaitGetNextFileData()
        {
            return await Task.Run(() =>
            {
                if (_isWorkSessionOfLines)
                    _event.AwaitOneData();
                return _isWorkSessionOfLines;
            });
        }

        //重复使用缓冲区，减少内存碎片
        byte[] _fileBuffer = new byte[FILE_BUFFER_SIZE];
        private async Task<byte[]> ReadAsync(System.IO.FileStream fileStream)
        {
            int lenght = await fileStream.ReadAsync(_fileBuffer, 0, _fileBuffer.Length);
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
        public void SetUploadFileStatus(SessionHandler session)
        {
            LogHelper.DebugWriteLog("SetUploadFileStatus head:" + string.Join(",", session.CompletedBuffer.Take(2).Select(c => c.ToString()).ToArray()));
            _event.SetOneData(session.CompletedBuffer);
        }
        private async Task<FileUploadFileStatus> AwaitUploadFileStatus()
        {
            return await Task.Run(() =>
            {
                //LogHelper.DebugWriteLog("get status");
                if (_isWorkSessionOfLines)
                {
                    var data = _event.AwaitOneData();
                    if (data.IsNullOrEmpty())
                        return null;
                    LogHelper.DebugWriteLog("AwaitUploadFileStatus head:" + string.Join(",", data.Take(2).Select(c => c.ToString()).ToArray()));
                    return data.GetMessageEntity<FileUploadFileStatus>();
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
                if (_adapter.WindowClosed)
                    return false;
                else
                    return true;
            });
        }

        private void TransferNotify(FileTransferNotify notify, string fileName, long position, long fileSize)
        {
            if (_adapter.WindowClosed)//UI未关闭时才允许操作控件
                return;

            switch (notify)
            {
                case FileTransferNotify.Begin:
                    if (fileSize > 0)
                        this.transferProgress.Value = Convert.ToInt32(position / (float)fileSize * 100);
                    this.transferDatalenght.Text = $"已传输{FileHelper.LengthToFileSize(position)}";
                    this.time.Text = "传输时间:{0}s".FormatTo((DateTime.Now - _startTime).TotalSeconds.ToString("0"));
                    break;
                case FileTransferNotify.Transfering:
                    this.transferProgress.Value = Convert.ToInt32(position / (float)fileSize * 100);
                    this.transferCaption.Text = $"正在传输:{Path.GetFileName(fileName)} 文件大小:{FileHelper.LengthToFileSize(fileSize)}";
                    this.transferDatalenght.Text = $"已传输{FileHelper.LengthToFileSize(position)}";
                    this.time.Text = "传输时间:{0}s".FormatTo((DateTime.Now - _startTime).TotalSeconds.ToString("0"));
                    break;
                case FileTransferNotify.End:
                    this.transferProgress.Value = 0;
                    this.transferCaption.Text = $"目录装载完成";
                    this.transferDatalenght.Text = "已传输0KB";
                    break;
            }

        }

        private async Task DownloadDirectory(string remotedirectory, string localdirectory)
        {
            reset:
            this._filesQueue.Clear();
            _taskStatus = 1;
            _adapter.SendAsyncMessage(MessageHead.S_FILE_GETDIR_FILES,
                new FileDirectoryGetFilesPack()
                {
                    DirectoryPath = remotedirectory
                });

            var result = await this.AwaitGetDirectoryFiles();
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

                if (_taskStatus == 0)
                    break;//停止任务

                var file = _filesQueue.Dequeue();
                if (file.Type == DirectoryFileType.File)
                {
                    var targetFileName = file.FileName;
                    var localFileName = Path.Combine(localdirectory, file.FileName.Substring(remotedirectory.LastIndexOf("\\") + 1));
                    await this.DownloadFile(targetFileName, localFileName);
                    if (_transferMode == FileTransferModeForm.TransferMode.Cancel || _taskStatus == 0)
                        break;
                }
                else
                {
                    var directory = Path.Combine(localdirectory, file.FileName.Substring(remotedirectory.LastIndexOf("\\") + 1));
                    if (!FileHelper.VerifyLongPath(directory))
                    {
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                    }
                }

            }
            this._filesQueue.Clear();
            this.transferCaption.Text = "目录下载完成!";

        }
        [PacketHandler(MessageHead.C_FILE_DIR_FILES)]
        public void SetFilesTriggerEvent(SessionHandler session)
        {
            foreach (var file in session.CompletedBuffer.GetMessageEntity<FileDirectoryFilesPack>().Files)
                _filesQueue.Enqueue(file);
            _filesTrigger.Set();
        }

        private async Task<bool> AwaitGetDirectoryFiles()
        {
            return await Task.Run(() =>
            {
                if (this._isWorkSessionOfLines)
                {
                    _filesTrigger.Reset();
                    _filesTrigger.WaitOne();
                }

                return this._isWorkSessionOfLines;
            });
        }

        private async Task<bool> AwaitResetDownloadDirectory()
        {
            return await Task.Run(() =>
            {
                _sessionOfLinesEvent.WaitOne();//等待重连
                if (_adapter.WindowClosed)
                    return false;
                else
                    return true;
            });
        }
        private async Task UploadDirectoryFiles(string localdirectory, string remotedirectory)
        {
            try
            {
                if (_taskStatus == 0 || _transferMode == FileTransferModeForm.TransferMode.Cancel)
                    return;

                string[] files = Directory.GetFiles(localdirectory);
                foreach (var file in files)
                {
                    var targetFileName = Path.Combine(remotedirectory, file.Substring(localdirectory.LastIndexOf("\\") + 1));
                    await this.UploadFile(file, targetFileName);
                    if (_taskStatus == 0 || _transferMode == FileTransferModeForm.TransferMode.Cancel)
                        return;
                }
                string[] directroys = Directory.GetDirectories(localdirectory);
                if (files.Length <= 0 && directroys.Length <= 0)
                {
                    string targetdirectoy = localdirectory.Substring(localdirectory.LastIndexOf("\\") + 1);
                    this.RemoteCreateDirectory(targetdirectoy, true);
                    return;
                }
                foreach (var file in directroys)
                {
                    var targetdirectory = Path.Combine(remotedirectory, localdirectory.Substring(localdirectory.LastIndexOf("\\") + 1));
                    await this.UploadDirectoryFiles(file, targetdirectory);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }


        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            this._taskStatus = 0;
        }

        private void savePath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(txtSavePath.Text))
                Process.Start(txtSavePath.Text);
        }

        private void 刷新目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2_Click(null, null);
        }

        private void 详细信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.详细信息ToolStripMenuItem.Checked = true;
            this.列表ToolStripMenuItem.Checked = false;
            this.平铺ToolStripMenuItem.Checked = false;
            this.fileList.View = View.Details;
            this.fileList.CheckBoxes = true;
        }

        private void 列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.详细信息ToolStripMenuItem.Checked = false;
            this.列表ToolStripMenuItem.Checked = true;
            this.平铺ToolStripMenuItem.Checked = false;
            this.fileList.View = View.List;
            this.fileList.CheckBoxes = true;
        }

        private void 平铺ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.详细信息ToolStripMenuItem.Checked = false;
            this.列表ToolStripMenuItem.Checked = false;
            this.平铺ToolStripMenuItem.Checked = true;

            this.fileList.CheckBoxes = false;
            this.fileList.View = View.Tile;
        }

        private async void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtRemotedirectory.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxError("当前路径不能上传!");
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.ShowDialog();
            if (dlg.FileNames.Length > 0)
            {
                this.downloadMenuItem.Enabled = false;
                this.uploadMenuItem.Enabled = false;
                this._transferMode = null;
                this._taskStatus = 1;
                this._startTime = DateTime.Now;
                var remotedirectory = txtRemotedirectory.Text;
                foreach (var file in dlg.FileNames)
                {
                    await this.UploadFile(file, Path.Combine(remotedirectory, Path.GetFileName(file)));
                    if (_transferMode == FileTransferModeForm.TransferMode.Cancel || _taskStatus == 0)
                        break;
                }
                this.GetFiles(remotedirectory);
                this.downloadMenuItem.Enabled = true;
                this.uploadMenuItem.Enabled = true;
            }
        }

        private async void 文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtRemotedirectory.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxError("当前路径不能上传!");
                return;
            }

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if (dlg.SelectedPath != "" && result == DialogResult.OK)
            {
                if (dlg.SelectedPath == Path.GetPathRoot(dlg.SelectedPath))
                    MessageBoxHelper.ShowBoxError("暂不支持根目录的发送,请重新选择文件夹!");
                else
                {
                    this._transferMode = null;
                    this._taskStatus = 1;//开始状态
                    this._startTime = DateTime.Now;
                    var remotedirectory = txtRemotedirectory.Text;

                    this.downloadMenuItem.Enabled = false;
                    this.uploadMenuItem.Enabled = false;

                    await this.UploadDirectoryFiles(dlg.SelectedPath, remotedirectory);

                    this.downloadMenuItem.Enabled = true;
                    this.uploadMenuItem.Enabled = true;
                    this.GetFiles(remotedirectory);
                    this.transferCaption.Text = "目录上传完成!";
                }
            }
        }

        private void fileList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private async void fileList_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (txtRemotedirectory.Text.IsNullOrEmpty())
                {
                    MessageBoxHelper.ShowBoxError("当前路径不能上传!");
                    return;
                }
                string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                if (files.Any(c => Path.GetPathRoot(c) == c))
                {
                    MessageBoxHelper.ShowBoxError("暂不支持根目录的发送,请重新选择文件夹!");
                    return;
                }
                this.downloadMenuItem.Enabled = false;
                this.uploadMenuItem.Enabled = false;
                this._transferMode = null;
                this._taskStatus = 1;
                this._startTime = DateTime.Now;
                var remotedirectory = txtRemotedirectory.Text;
                foreach (var file in files)
                {
                    if (Directory.Exists(file))
                        await this.UploadDirectoryFiles(file, remotedirectory);
                    else
                        await this.UploadFile(file, Path.Combine(remotedirectory, Path.GetFileName(file)));

                    if (_transferMode == FileTransferModeForm.TransferMode.Cancel || _taskStatus == 0)
                        break;
                }
                this.GetFiles(remotedirectory);
                this.downloadMenuItem.Enabled = true;
                this.uploadMenuItem.Enabled = true;
            }
            catch { }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowDialog();
            if (dlg.SelectedPath != "")
            {
                IniConfigHelper.SetValue("RemoteFile", "SavaFilePath", dlg.SelectedPath, Path.Combine(Environment.CurrentDirectory, "SiMayConfig.ini"));
                txtSavePath.Text = dlg.SelectedPath;
            }
        }
        private void ToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedItems.Count > 0)
            {
                var file = fileList.Items[fileList.SelectedItems[0].Index] as FileListViewItem;
                if (MessageBox.Show("确定要运行 " + file.FileName + " ?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                    return;
                this.RemoteExecuteFile(Path.Combine(txtRemotedirectory.Text, file.FileName));
            }
        }

        private void 下载到ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowDialog();
            if (dlg.SelectedPath != "")
            {
                this.StartDownload(txtRemotedirectory.Text, dlg.SelectedPath);
            }
        }

        private void remoteDirectoryTreeView_DoubleClick(object sender, EventArgs e)
        {
            var node = this._remoteDirectoryTreeView.SelectedNode;

            if (node == null)
                return;

            if (node.Nodes.Count > 0)
                node.Nodes.Clear();

            var path = node.FullPath;

            _adapter.SendAsyncMessage(MessageHead.S_FILE_TREE_DIR,
                new FileGetTreeDirectoryPack()
                {
                    TargetRoot = path
                });

            this._remoteDirectoryTreeView.Enabled = false;
        }

        [PacketHandler(MessageHead.C_FILE_TREE_DIRS)]
        public void TreeFilesHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<FileTreeDirFilePack>();
            if (_remoteDirectoryTreeView.Nodes.Count <= 0)
            {
                foreach (var file in pack.FileList)
                {
                    var node = new TreeNode(file.FileName, IcoIndex("", true), IcoIndex("", true));
                    _remoteDirectoryTreeView.Nodes.Add(file.FileName);
                }
            }
            else
            {
                var node = this._remoteDirectoryTreeView.SelectedNode;
                if (!pack.FileList.IsNullOrEmpty())
                    node.Nodes.AddRange(pack.FileList.Select(v => new TreeNode(v.FileName, IcoIndex("DIR", false), IcoIndex("DIR", false))).ToArray());

                node.Expand();

                this._remoteDirectoryTreeView.Enabled = true;
            }
        }
        private void 打开目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = this._remoteDirectoryTreeView.SelectedNode;
            this.txtRemotedirectory.Text = node.FullPath;
            this.GetFiles(node.FullPath);
            this._remoteDirectoryTreeView.Hide();
            this._closeTreeBtn.Hide();
        }

        private void TxtRemotedirectory_MouseClick(object sender, MouseEventArgs e)
        {
            _remoteDirectoryTreeView.Top = txtRemotedirectory.Top + txtRemotedirectory.Height;
            _remoteDirectoryTreeView.Left = txtRemotedirectory.Left;
            _remoteDirectoryTreeView.Width = txtRemotedirectory.Width;
            _remoteDirectoryTreeView.Height = 300;
            _remoteDirectoryTreeView.BringToFront();
            _remoteDirectoryTreeView.Show();

            _closeTreeBtn.Top = _remoteDirectoryTreeView.Top + _remoteDirectoryTreeView.Height;
            _closeTreeBtn.Left = _remoteDirectoryTreeView.Left + _remoteDirectoryTreeView.Width - _closeTreeBtn.Width;
            _closeTreeBtn.BringToFront();
            _closeTreeBtn.Show();
        }
        private void _closeTreeBtn_Click(object sender, EventArgs e)
        {
            this._closeTreeBtn.Hide();
            this._remoteDirectoryTreeView.Hide();
        }

        private void TxtRemotedirectory_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                this.GetFiles(txtRemotedirectory.Text);
        }

        public void Start()
        {
            MessageBoxHelper.ShowBoxError("未完成的功能开发!");
            throw new NotImplementedException();
        }

        public void SessionClose(AdapterHandlerBase handler)
        {
            throw new NotImplementedException();
        }

        public void ContinueTask(AdapterHandlerBase handler)
        {
            throw new NotImplementedException();
        }
    }
}