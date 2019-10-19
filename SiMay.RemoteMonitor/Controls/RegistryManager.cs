using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.Reg;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.MainForm;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    [Disable]
    [ControlApp(50, "注册表管理", "RegEditManagerJob", "RegEditManager")]
    public partial class RegistryManager : Form, IControlSource
    {
        private ImageList _imgList = new ImageList();
        private Hashtable _icoHash = new Hashtable();
        private FileIconUtil _iconUtil = new FileIconUtil();

        private string _title = "//远程注册表管理 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public RegistryManager(MessageAdapter adapter)
        {
            _adapter = adapter;
            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();
        private void Adapter_OnSessionNotifyPro(SessionHandler session, Notify.SessionNotifyType notify)
        {
            switch (notify)
            {
                case Notify.SessionNotifyType.Message:
                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case Notify.SessionNotifyType.OnReceive:
                    break;
                case Notify.SessionNotifyType.ContinueTask:
                    this.Text = _title;
                    break;
                case Notify.SessionNotifyType.SessionClosed:
                    this.Text = _title + " [" + _adapter.TipText + "]";
                    break;
                case Notify.SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case Notify.SessionNotifyType.WindowClose:
                    _adapter.WindowClosed = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        [PacketHandler(MessageHead.C_REG_CREATESUBKEY_FINSH)]
        public void CreateSubkeyFinishHandler(SessionHandler session)
        {
            var result = session.CompletedBuffer.GetMessageEntity<RegOperFinshPack>();
            var node = list.SelectedNode;

            if (node == null)
                return;

            if (result.Result)
            {
                node.Nodes.Add(result.Value);
                MessageBoxHelper.ShowBoxExclamation(result.Value + " 创建成功!");
            }

            list.Enabled = true;
        }

        [PacketHandler(MessageHead.C_REG_DELETESUBKEY_FINSH)]
        public void DeleteSubKeyFinshHandler(SessionHandler session)
        {
            var result = session.CompletedBuffer.GetMessageEntity<RegOperFinshPack>();
            if (result.Result)
                list.SelectedNode.Remove();

            list.Enabled = true;
        }

        [PacketHandler(MessageHead.C_REG_VALUENAMES)]
        public void SubValueDataHandler(SessionHandler session)
        {
            var valuePack = session.CompletedBuffer.GetMessageEntity<RegValuesPack>();
            if (!valuePack.Values.IsNullOrEmpty())
                this.LoadViewValues(valuePack.Values);
        }
        private void LoadViewValues(RegValueItem[] values)
        {
            valueView.Items.Clear();
            foreach (var value in values)
            {
                var lv = new ListViewItem();
                lv.Text = value.ValueName;
                lv.SubItems.Add(value.Value);

                valueView.Items.Add(lv);
            }
        }
        [PacketHandler(MessageHead.C_REG_ROOT_DIRSUBKEYNAMES)]
        public void DirectorySubKeysDataHandler(SessionHandler session)
        {
            var dirs = session.CompletedBuffer.GetMessageEntity<RegRootDirectorysPack>();
            list.Nodes.Clear();
            list.Nodes.AddRange(dirs.RootDirectorys.Select(x => new TreeNode(x)).ToArray());
            obNumber.Text = dirs.RootDirectorys.Length.ToString();
        }

        [PacketHandler(MessageHead.C_REG_SUBKEYNAMES)]
        public void SubKeyDataHandler(SessionHandler session)
        {
            var subKeys = session.CompletedBuffer.GetMessageEntity<RegSubKeyValuePack>();
            var node = list.SelectedNode;
            if (!subKeys.SubKeyNames.IsNullOrEmpty())
                node.Nodes.AddRange(subKeys.SubKeyNames.Select(v => new TreeNode(v)).ToArray());

            node.Expand();

            if (!subKeys.Values.IsNullOrEmpty())
            {
                this.LoadViewValues(subKeys.Values);
                obNumber.Text = subKeys.SubKeyNames.Length.ToString();
            }

            list.Enabled = true;
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

        private void RegistryManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            _adapter.WindowClosed = true;
            _handlerBinder.Dispose();
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void RegistryManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;

            list.ImageList = _imgList;
            list.ImageIndex = IcoIndex("DIR", false);

            directoryNames.Items.Add("HKEY-CLASS-ROOT");
            directoryNames.Items.Add("HKEY-CURRENT-USER");
            directoryNames.Items.Add("HKEY-LOCAL-MACHINE");
            directoryNames.Items.Add("HKEY-USER");
            directoryNames.Items.Add("HKEY-CURRENT-CONFIG");
        }

        private void directoryNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            _adapter.SendAsyncMessage(MessageHead.S_REG_OPENDIRECTLY, directoryNames.Text);
        }

        private void 编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (valueView.SelectedItems.Count < 1)
            {
                MessageBoxHelper.ShowBoxExclamation("请选择要编辑的对象!");
                return;
            }

            var name = valueView.Items[valueView.SelectedItems[0].Index].SubItems[0].Text;
            var value = valueView.Items[valueView.SelectedItems[0].Index].SubItems[1].Text;
            RegistryEditForm dlg = new RegistryEditForm();
            dlg.KeyName = name;
            dlg.Value = value;

            dlg.KeyNameEnabled = false;
            dlg.ShowDialog();
            if (dlg.DialogResult == DialogResult.OK)
            {
                _adapter.SendAsyncMessage(MessageHead.S_REG_CREATEVALUE,
                    new RegNewValuePack()
                    {
                        Root = directoryNames.Text,
                        NodePath = path.Text,
                        ValueName = dlg.KeyName,
                        Value = dlg.Value
                    });
            }
        }

        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var node = list.SelectedNode;
            if (node == null)
                return;

            path.Text = node.FullPath;

            var result = MessageBox.Show("确定要删除 " + path.Text + " ?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                _adapter.SendAsyncMessage(MessageHead.S_REG_DELETESUBKEY, new RegDeleteSubKeyPack()
                {
                    Root = directoryNames.Text,
                    NodePath = path.Text
                });

                list.Enabled = false;
            }
        }

        private void list_DoubleClick(object sender, EventArgs e)
        {
            var node = list.SelectedNode;

            if (node == null)
                return;

            if (node.Nodes.Count > 0)
                node.Nodes.Clear();

            path.Text = node.FullPath;

            _adapter.SendAsyncMessage(MessageHead.S_REG_OPENSUBKEY,
                new RegOpenSubKeyPack()
                {
                    Root = directoryNames.Text,
                    NodePath = path.Text
                });

            list.Enabled = false;
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = list.SelectedNode;
            if (node == null)
                return;

            path.Text = node.FullPath;

            EnterForm dlg = new EnterForm();
            dlg.Caption = "正在往 " + path.Text + " 中创建子项! 请输入项名称";
            if (dlg.ShowDialog() == DialogResult.OK || dlg.Value != "")
            {
                if (dlg.Value == "")
                {
                    MessageBoxHelper.ShowBoxExclamation("子项名称不能为空!");
                    return;
                }

                foreach (TreeNode item in node.Nodes)
                {
                    if (item.Text == dlg.Value)
                    {
                        MessageBoxHelper.ShowBoxExclamation(dlg.Value + "已存在,创建失败!");
                        return;
                    }
                }

                _adapter.SendAsyncMessage(MessageHead.S_REG_CREATESUBKEY,
                    new RegNewSubkeyPack()
                    {
                        Root = directoryNames.Text,
                        NodePath = path.Text,
                        NewSubKeyName = dlg.Value
                    });

                list.Enabled = false;
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (valueView.SelectedItems.Count < 1)
            {
                MessageBoxHelper.ShowBoxExclamation("请选择要删除的对象!");
                return;
            }

            var name = valueView.Items[valueView.SelectedItems[0].Index].SubItems[0].Text;

            var result = MessageBox.Show("确定要删除 " + name + " ?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                _adapter.SendAsyncMessage(MessageHead.S_REG_DELETEVALUE,
                    new RegDeleteValuePack()
                    {
                        Root = directoryNames.Text,
                        NodePath = path.Text,
                        ValueName = name
                    });
            }
        }

        private void 创建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (directoryNames.Text.IsNullOrEmpty() || path.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxExclamation("此节点不能创建键值!");
                return;
            }
            RegistryEditForm dlg = new RegistryEditForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.KeyName.IsNullOrEmpty())
                {
                    MessageBoxHelper.ShowBoxExclamation("键值不能为空!");
                    return;
                }
                _adapter.SendAsyncMessage(MessageHead.S_REG_CREATEVALUE,
                    new RegNewValuePack()
                    {
                        Root = directoryNames.Text,
                        NodePath = path.Text,
                        ValueName = dlg.KeyName,
                        Value = dlg.Value
                    });
            }
        }

    }
}