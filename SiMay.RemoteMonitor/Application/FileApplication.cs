using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Enums;
using SiMay.Core.Packets;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteControlsCore.Interface;
using SiMay.RemoteMonitor.Application.FileCommon;
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
    [Application(typeof(RemoteFileAdapterHandler), AppJobConstant.REMOTE_FILE, 10)]
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

        private string _title = "//远程文件管理 #Name#";

        private TransferMode? _transferMode = null;
        private string[] _copyFileNames = null;

        private TreeView _remoteDirectoryTreeView;
        private Button _closeTreeBtn;
        private DateTime _startTime;//任务开始时间
        private ImageList _imgList = new ImageList();
        private Hashtable _icoHash = new Hashtable();
        private FileIconUtil _iconUtil = new FileIconUtil();
        public FileApplication()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Show();
        }

        public void SessionClose(AdapterHandlerBase handler)
        {
            this.Text = this._title + " [" + this.RemoteFileAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(AdapterHandlerBase handler)
        {
            this.Text = this._title;
        }

        private void FileManager_Load(object sender, EventArgs e)
        {
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

            this.Text = this._title = this._title.Replace("#Name#", this.RemoteFileAdapterHandler.OriginName);
            this.RemoteFileAdapterHandler.OnRemoteExceptionEventHandler += OnRemoteExceptionEventHandler;
            this.RemoteFileAdapterHandler.OnFileItemsEventHandler += OnFileItemsEventHandler;
            this.RemoteFileAdapterHandler.OnOpenTextEventHandler += OnOpenTextEventHandler;
            this.RemoteFileAdapterHandler.OnFileDeteledFinishEventHandler += OnFileDeteledFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileNameRenameFinishEventHandler += OnFileNameRenameFinishEventHandler;
            this.RemoteFileAdapterHandler.OnPasterFinishEventHandler += OnPasterFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileTransferProgressEventHandler += OnFileTransferProgressEventHandler;
            this.RemoteFileAdapterHandler.OnDirectoryCreateFinishEventHandler += OnDirectoryCreateFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileTreeItemsEventHandler += OnFileTreeItemsEventHandler;
            this.RemoteFileAdapterHandler.GetRemoteRootTreeItems(string.Empty);
            this.RemoteFileAdapterHandler.GetRemoteDriveItems();
        }
        private void Initialize()
        {
            string downPath = Path.Combine(Environment.CurrentDirectory, "download");

            if (!Directory.Exists(downPath))
                Directory.CreateDirectory(downPath);

            this.txtSavePath.Text = downPath;
            this.fileList.SmallImageList = _imgList;
            this.fileList.LargeImageList = _imgList;

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
                        this.RemoteFileAdapterHandler.GetRemoteSystemFoldFiles(Environment.SpecialFolder.DesktopDirectory);
                        break;
                    case IDM_DIR_DOC:
                        this.RemoteFileAdapterHandler.GetRemoteSystemFoldFiles(Environment.SpecialFolder.MyDocuments);
                        break;
                    case IDM_DIR_MUSIC:
                        this.RemoteFileAdapterHandler.GetRemoteSystemFoldFiles(Environment.SpecialFolder.MyMusic);
                        break;
                    case IDM_DIR_PIC:
                        this.RemoteFileAdapterHandler.GetRemoteSystemFoldFiles(Environment.SpecialFolder.MyPictures);
                        break;
                    case IDM_DIR_VIDEO:
                        this.RemoteFileAdapterHandler.GetRemoteSystemFoldFiles(Environment.SpecialFolder.MyVideos);
                        break;
                    case IDM_DIR_HOME:
                        this.RemoteFileAdapterHandler.GetRemoteDriveItems();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void OnFileItemsEventHandler(RemoteFileAdapterHandler adapterHandler, FileItem[] fileItems, string root, bool isSuccess, string message)
        {
            this.fileList.Items.Clear();

            if (!isSuccess)
                MessageBoxHelper.ShowBoxExclamation(message);

            this.txtRemotedirectory.Text = root;

            foreach (var file in fileItems)
                this.AddItem(file);

            this.transferCaption.Text = "装载目录" + txtRemotedirectory.Text + "完成,共 " + fileList.Items.Count.ToString() + " 个对象";
        }

        private void OnRemoteExceptionEventHandler(RemoteFileAdapterHandler adapterHandler, DateTime occurredTime, string tipMessage, string exceptionMessage, string stackTrace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("remoteService Exception:");
            sb.AppendLine("occurrence time:" + occurredTime.ToString());
            sb.AppendLine("TipMessage:" + tipMessage);
            sb.AppendLine("ExceptionMessage:" + exceptionMessage);
            sb.AppendLine("StackTrace:" + stackTrace);
            LogHelper.WriteErrorByCurrentMethod(sb.ToString());
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
            this.RemoteFileAdapterHandler.OnRemoteExceptionEventHandler -= OnRemoteExceptionEventHandler;
            this.RemoteFileAdapterHandler.OnFileItemsEventHandler -= OnFileItemsEventHandler;
            this.RemoteFileAdapterHandler.OnOpenTextEventHandler -= OnOpenTextEventHandler;
            this.RemoteFileAdapterHandler.OnFileDeteledFinishEventHandler -= OnFileDeteledFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileNameRenameFinishEventHandler -= OnFileNameRenameFinishEventHandler;
            this.RemoteFileAdapterHandler.OnPasterFinishEventHandler -= OnPasterFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileTransferProgressEventHandler -= OnFileTransferProgressEventHandler;
            this.RemoteFileAdapterHandler.OnDirectoryCreateFinishEventHandler -= OnDirectoryCreateFinishEventHandler;
            this.RemoteFileAdapterHandler.OnFileTreeItemsEventHandler -= OnFileTreeItemsEventHandler;
            this.RemoteFileAdapterHandler.CloseHandler();
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
                    this.RemoteFileAdapterHandler.RemoteOpenText(Path.Combine(txtRemotedirectory.Text, file.FileName));
                }
                else
                {
                    var currentPath = txtRemotedirectory.Text;
                    if (currentPath.IsNullOrEmpty())
                        currentPath = file.FileName;
                    else
                        currentPath = Path.Combine(currentPath, file.FileName);
                    this.RemoteFileAdapterHandler.GetRemoteFiles(currentPath);
                }
            }
        }

        private void OnOpenTextEventHandler(RemoteFileAdapterHandler adapterHandler, string text, bool isSuccess)
        {
            if (!isSuccess)
            {
                MessageBoxHelper.ShowBoxExclamation("远程文件打开失败!");
                return;
            }
            var tmp = Path.Combine(Environment.CurrentDirectory, "tmp");
            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            string randomName = Guid.NewGuid().ToString() + ".txt";
            File.WriteAllText(Path.Combine(tmp, randomName), text);

            Process.Start(Path.Combine(tmp, randomName));
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!txtRemotedirectory.Text.IsNullOrEmpty())
                this.RemoteFileAdapterHandler.GetRemoteFiles(txtRemotedirectory.Text);
            else
            {
                this.RemoteFileAdapterHandler.GetRemoteDriveItems();
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
                    this.RemoteFileAdapterHandler.GetRemoteDriveItems();
                }
                else
                    this.RemoteFileAdapterHandler.GetRemoteFiles(newPath);
            }
            catch (Exception)
            {
                this.RemoteFileAdapterHandler.GetRemoteDriveItems();
            }
        }

        private void m_files_DoubleClick(object sender, EventArgs e)
        {
            this.OnSelectRemoteOpenFile();
        }

        private IEnumerable<FileListViewItem> GetSelectFiles()
        {
            if (fileList.SelectedItems.Count != 0)
            {
                var selectItems = fileList.SelectedItems;
                foreach (ListViewItem item in selectItems)
                    item.Checked = true;
                for (int i = 0; i < fileList.Items.Count; i++)
                {
                    if (fileList.Items[i].Checked)
                    {
                        var item = fileList.Items[i].ConvertTo<FileListViewItem>();
                        yield return item;
                        item.Checked = false;
                    }
                }
            }
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

            this.RemoteFileAdapterHandler.RemoteDeleteFiles(files.Select(c => Path.Combine(txtRemotedirectory.Text, c.FileName)).ToArray());

        }

        private void OnFileDeteledFinishEventHandler(RemoteFileAdapterHandler adapterHandler, string[] deletedFiles)
        {
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                FileListViewItem item = fileList.Items[i] as FileListViewItem;
                foreach (var file in deletedFiles)
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

        private bool RemoteCreateDirectory(string path)
        {
            if (FileHelper.VerifyLongPath(path))
            {
                LogHelper.WriteErrorByCurrentMethod("Create Directory 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。remote:{0}".FormatTo(path));
                return false;
            }

            this.RemoteFileAdapterHandler.RemoteCreateDirectory(path, false);

            return true;
        }
        private void OnDirectoryCreateFinishEventHandler(RemoteFileAdapterHandler adapterHandler, bool isSuccess)
        {
            if (isSuccess)
                this.RemoteFileAdapterHandler.GetRemoteFiles(txtRemotedirectory.Text);
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
                        this.RemoteFileAdapterHandler.RemoteFileRename(Path.Combine(txtRemotedirectory.Text, file.FileName), Path.Combine(txtRemotedirectory.Text, dlg.Value));
                    }
                    else
                    {
                        MessageBoxHelper.ShowBoxError("文件名不能包含 | / \\ * ? \" < > : 等特殊符号!");
                    }
                }

            }
        }

        private void OnFileNameRenameFinishEventHandler(RemoteFileAdapterHandler adapterHandler, string srcNamec, string targetName, bool isSuccess)
        {
            if (isSuccess)
            {
                for (int i = 0; i < fileList.Items.Count; i++)
                {
                    FileListViewItem item = fileList.Items[i] as FileListViewItem;
                    if (item.FileName == Path.GetFileName(srcNamec))
                    {
                        item.Text = Path.GetFileName(targetName);
                        item.FileName = Path.GetFileName(targetName);
                        break;
                    }

                }
            }
            else
                MessageBoxHelper.ShowBoxError("文件重命名失败!");
        }

        private void 复制文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var files = this.GetSelectFiles().ToList();
            if (files.Any(c => c.FileType == FileItemType.Disk))
            {
                MessageBoxHelper.ShowBoxError("磁盘对象不支持复制!");
                return;
            }
            if (files.Count > 0)
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

            this.RemoteFileAdapterHandler.RemoteFilePaster(txtRemotedirectory.Text, _copyFileNames);
            transferCaption.Text = "已向远程发出粘贴请求,请等待完成反馈!";
        }

        private void OnPasterFinishEventHandler(RemoteFileAdapterHandler adapterHandler, string[] unsuccessfulFiles)
        {
            this.RemoteFileAdapterHandler.GetRemoteFiles(txtRemotedirectory.Text);
            if (unsuccessfulFiles.Length > 0)
                MessageBoxHelper.ShowBoxExclamation($"文件复制完成，但有部分文件复制操作异常:{string.Join(", ", unsuccessfulFiles)},请检查!");
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
            this.RemoteFileAdapterHandler.TransferTaskFlage = TransferTaskFlage.Allow;
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
                if (this._transferMode == TransferMode.Cancel ||//选择取消传输
                    this.RemoteFileAdapterHandler.TransferTaskFlage == TransferTaskFlage.Abort ||//终止传输信号
                    this.RemoteFileAdapterHandler.IsClose)//关闭应用
                    break;
            }
            this.downloadMenuItem.Enabled = true;
            this.uploadMenuItem.Enabled = true;
            this.downloadAsToolStripMenuItem.Enabled = true;
        }

        private IFileStream ByLocalFileChooseTransferMode(string localFileName)
        {

            IFileStream ifileStream = null;
            if (FileHelper.VerifyLongPath(localFileName))
            {
                LogHelper.WriteErrorByCurrentMethod("localFileName 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。local:{0}".FormatTo(localFileName));
                return ifileStream;
            }

            if (!Directory.Exists(Path.GetDirectoryName(localFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(localFileName));

            long position = 0;
            if (File.Exists(localFileName))
            {
                TransferMode transferMode = TransferMode.Continuingly;
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
                    case TransferMode.Replace:
                        File.Delete(localFileName);
                        break;
                    case TransferMode.ReplaceAll:
                        File.Delete(localFileName);
                        _transferMode = TransferMode.Replace;
                        break;
                    case TransferMode.Continuingly:
                        position = new FileInfo(localFileName).Length;
                        break;
                    case TransferMode.ContinuinglyAll:
                        position = new FileInfo(localFileName).Length;
                        _transferMode = TransferMode.Continuingly;
                        break;
                    case TransferMode.JumpOver:
                        return ifileStream;//跳过本次
                    case TransferMode.Cancel:
                        _transferMode = transferMode;
                        return ifileStream;
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
                return ifileStream;
            }
            return new WindowsForFileStream(fileStream);
        }

        private async Task DownloadFile(string remoteFileName, string localFileName)
        {
            if (FileHelper.VerifyLongPath(remoteFileName))
            {
                LogHelper.WriteErrorByCurrentMethod("remoteFileName 指定的路径或文件名太长，或者两者都太长。完全限定文件名必须少于 260 个字符，并且目录名必须少于 248 个字符。local:{0},remote:{1}".FormatTo(localFileName, remoteFileName));
                return;
            }

            var fileStream = this.ByLocalFileChooseTransferMode(localFileName);
            if (fileStream == null)
                return;

            await this.RemoteFileAdapterHandler.DownloadFile(fileStream, remoteFileName);
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

            await this.RemoteFileAdapterHandler.UploadFile(new WindowsForFileStream(fileStream),
                remoteFileName, r =>
                {
                    TransferMode transferMode = TransferMode.Continuingly;
                    if (!_transferMode.HasValue)
                    {
                        FileTransferModeForm dlg = new FileTransferModeForm();
                        dlg.TipMessage = "此远程文件夹已包含一个名为 " + Path.GetFileName(r) + " 的文件";
                        dlg.ShowDialog();
                        transferMode = dlg.TransferModeResult;
                    }
                    else
                        transferMode = _transferMode.Value;
                    return transferMode;
                });
        }

        private void OnFileTransferProgressEventHandler(RemoteFileAdapterHandler adapterHandler, FileTransferFlag state, string fileName, long position, long fileSize)
        {
            if (this.RemoteFileAdapterHandler.IsClose)//UI未关闭时才允许操作控件
                return;

            switch (state)
            {
                case FileTransferFlag.Begin:
                    if (fileSize > 0)
                        this.transferProgress.Value = Convert.ToInt32(position / (float)fileSize * 100);
                    this.transferDatalenght.Text = $"已传输{FileHelper.LengthToFileSize(position).PadRight(10)}";
                    this.time.Text = "传输时间:{0}s".FormatTo((DateTime.Now - _startTime).TotalSeconds.ToString("0"));
                    break;
                case FileTransferFlag.Transfering:
                    this.transferProgress.Value = Convert.ToInt32(position / (float)fileSize * 100);
                    this.transferCaption.Text = $"正在传输:{Path.GetFileName(fileName)} 文件大小:{FileHelper.LengthToFileSize(fileSize)}";
                    this.transferDatalenght.Text = $"已传输:{FileHelper.LengthToFileSize(position).PadRight(10)}";
                    this.time.Text = "传输时间:{0}s".FormatTo((DateTime.Now - _startTime).TotalSeconds.ToString("0"));
                    break;
                case FileTransferFlag.End:
                    this.transferProgress.Value = 0;
                    this.transferCaption.Text = $"目录装载完成";
                    this.transferDatalenght.Text = "已传输0KB";
                    break;
            }

        }
        private async Task DownloadDirectory(string remotedirectory, string localdirectory)
        {
            await this.RemoteFileAdapterHandler.DownloadDirectory(remotedirectory,
                remoteFileName =>
                {
                    var localFileName = Path.Combine(localdirectory, remoteFileName);
                    return this.ByLocalFileChooseTransferMode(localFileName);
                }, dirName =>
                {
                    var directory = Path.Combine(localdirectory, dirName);
                    if (!FileHelper.VerifyLongPath(directory))
                    {
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                    }
                    else
                    {
                        LogHelper.WriteErrorByCurrentMethod($"DownloadDirectory 文件夹路径过长:{directory}");
                    }
                });
            this.transferCaption.Text = "目录下载完成!";
        }

        private async Task UploadDirectoryFiles(string localdirectory, string remotedirectory)
        {
            try
            {
                if (this.RemoteFileAdapterHandler.TransferTaskFlage == TransferTaskFlage.Abort || _transferMode == TransferMode.Cancel)
                    return;

                string[] files = Directory.GetFiles(localdirectory);
                foreach (var file in files)
                {
                    var targetFileName = Path.Combine(remotedirectory, file.Substring(localdirectory.LastIndexOf("\\") + 1));
                    await this.UploadFile(file, targetFileName);
                    if (this.RemoteFileAdapterHandler.TransferTaskFlage == TransferTaskFlage.Abort || _transferMode == TransferMode.Cancel)
                        return;
                }
                string[] directroys = Directory.GetDirectories(localdirectory);
                if (files.Length <= 0 && directroys.Length <= 0)
                {
                    string targetdirectoy = localdirectory.Substring(localdirectory.LastIndexOf("\\") + 1);
                    this.RemoteFileAdapterHandler.RemoteCreateDirectory(targetdirectoy, true);
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

            this.transferCaption.Text = "目录上传完成!";
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            this.RemoteFileAdapterHandler.StopTransferTask();
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
                this.RemoteFileAdapterHandler.TransferTaskFlage = TransferTaskFlage.Allow;
                this._startTime = DateTime.Now;
                var remotedirectory = txtRemotedirectory.Text;
                foreach (var file in dlg.FileNames)
                {
                    await this.UploadFile(file, Path.Combine(remotedirectory, Path.GetFileName(file)));
                    if (_transferMode == TransferMode.Cancel || this.RemoteFileAdapterHandler.TransferTaskFlage == TransferTaskFlage.Abort)
                        break;
                }
                this.RemoteFileAdapterHandler.GetRemoteFiles(remotedirectory);
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
                    this.RemoteFileAdapterHandler.TransferTaskFlage = TransferTaskFlage.Allow;
                    this._startTime = DateTime.Now;
                    var remotedirectory = txtRemotedirectory.Text;

                    this.downloadMenuItem.Enabled = false;
                    this.uploadMenuItem.Enabled = false;

                    await this.UploadDirectoryFiles(dlg.SelectedPath, remotedirectory);

                    this.downloadMenuItem.Enabled = true;
                    this.uploadMenuItem.Enabled = true;
                    this.RemoteFileAdapterHandler.GetRemoteFiles(remotedirectory);
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
                this.RemoteFileAdapterHandler.TransferTaskFlage = TransferTaskFlage.Allow;
                this._startTime = DateTime.Now;
                var remotedirectory = txtRemotedirectory.Text;
                foreach (var file in files)
                {
                    if (Directory.Exists(file))
                        await this.UploadDirectoryFiles(file, remotedirectory);
                    else
                        await this.UploadFile(file, Path.Combine(remotedirectory, Path.GetFileName(file)));

                    if (_transferMode == TransferMode.Cancel || this.RemoteFileAdapterHandler.TransferTaskFlage == TransferTaskFlage.Abort)
                        break;
                }
                this.RemoteFileAdapterHandler.GetRemoteFiles(remotedirectory);
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
                this.RemoteFileAdapterHandler.RemoteExecuteFile(Path.Combine(txtRemotedirectory.Text, file.FileName));
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

            this.RemoteFileAdapterHandler.GetRemoteRootTreeItems(path);

            this._remoteDirectoryTreeView.Enabled = false;
        }


        private void OnFileTreeItemsEventHandler(RemoteFileAdapterHandler adapterHandler, FileItem[] fileItems)
        {
            if (_remoteDirectoryTreeView.Nodes.Count <= 0)
            {
                foreach (var file in fileItems)
                {
                    var node = new TreeNode(file.FileName, IcoIndex("", true), IcoIndex("", true));
                    _remoteDirectoryTreeView.Nodes.Add(file.FileName);
                }
            }
            else
            {
                var node = this._remoteDirectoryTreeView.SelectedNode;
                if (!fileItems.IsNullOrEmpty())
                    node.Nodes.AddRange(fileItems.Select(v => new TreeNode(v.FileName, IcoIndex("DIR", false), IcoIndex("DIR", false))).ToArray());

                node.Expand();

                this._remoteDirectoryTreeView.Enabled = true;
            }
        }
        private void 打开目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = this._remoteDirectoryTreeView.SelectedNode;
            this.txtRemotedirectory.Text = node.FullPath;
            this.RemoteFileAdapterHandler.GetRemoteFiles(node.FullPath);
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
                this.RemoteFileAdapterHandler.GetRemoteFiles(txtRemotedirectory.Text);
        }
    }
}