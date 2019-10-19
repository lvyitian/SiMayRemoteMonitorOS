using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.RegEdit;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using SiMay.RemoteMonitor.UserControls;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(50, "注册表管理", "RemoteRegistryEditorJob", "RegEditManager")]
    public partial class RegistryEditorForm : Form, IControlSource
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; //WS_EX_COMPOSITED
                return cp;
            }
        }

        private string _title = "//注册表管理 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();

        public RegistryEditorForm(MessageAdapter adapter)
        {
            _adapter = adapter;
            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();

        private void FrmRegistryEditor_Load(object sender, EventArgs e)
        {
            // signal client to retrive the root nodes (indicated by null)
            this.Text = _title;

            _adapter.SendAsyncMessage(MessageHead.S_NREG_LOAD_REGKEYS,
                new DoLoadRegistryKeyPack()
                {
                    RootKeyName = ""
                });
        }

        private void Adapter_OnSessionNotifyPro(SessionHandler session, SessionNotifyType notify)
        {
            switch (notify)
            {
                case SessionNotifyType.Message:
                    if (_adapter.WindowClosed)
                        return;

                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case SessionNotifyType.OnReceive:
                    break;
                case SessionNotifyType.ContinueTask:

                    this.Text = _title;
                    _adapter.SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);

                    break;
                case SessionNotifyType.SessionClosed:
                    this.Text = _title + " [" + _adapter.TipText + "]";
                    break;
                case SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case SessionNotifyType.WindowClose:

                    _adapter.WindowClosed = true;
                    this.Close();

                    break;
                default:
                    break;
            }
        }

        private void FrmRegistryEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void ShowErrorMessage(object sender, string errorMsg)
        {
            MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #region TreeView helper functions

        private void AddRootKey(RegSeekerMatch match)
        {
            TreeNode node = CreateNode(match.Key, match.Key, match.Data);
            node.Nodes.Add(new TreeNode());
            tvRegistryDirectory.Nodes.Add(node);
        }

        private TreeNode AddKeyToTree(TreeNode parent, RegSeekerMatch subKey)
        {
            TreeNode node = CreateNode(subKey.Key, subKey.Key, subKey.Data);
            parent.Nodes.Add(node);
            if (subKey.HasSubKeys)
                node.Nodes.Add(new TreeNode());
            return node;
        }

        private TreeNode CreateNode(string key, string text, object tag)
        {
            return new TreeNode()
            {
                Text = text,
                Name = key,
                Tag = tag
            };
        }

        [PacketHandler(MessageHead.C_NREG_LOAD_REGKEYS)]
        public void AddKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRegistryKeysResponsePack>();
            if (pack.IsError)
            {
                MessageBoxHelper.ShowBoxExclamation(pack.ErrorMsg);
                return;
            }
            if (string.IsNullOrEmpty(pack.RootKey))
            {
                tvRegistryDirectory.BeginUpdate();

                foreach (var match in pack.Matches)
                    AddRootKey(match);

                tvRegistryDirectory.SelectedNode = tvRegistryDirectory.Nodes[0];

                tvRegistryDirectory.EndUpdate();
            }
            else
            {
                TreeNode parent = GetTreeNode(pack.RootKey);

                if (parent != null)
                {
                    tvRegistryDirectory.BeginUpdate();

                    foreach (var match in pack.Matches)
                        AddKeyToTree(parent, match);

                    parent.Expand();
                    tvRegistryDirectory.EndUpdate();
                }
            }
        }

        [PacketHandler(MessageHead.C_NREG_CREATE_KEY_RESPONSE)]
        public void CreateNewKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetCreateRegistryKeyResponsePack>();
            TreeNode parent = GetTreeNode(pack.ParentPath);

            TreeNode node = AddKeyToTree(parent, pack.Match);

            node.EnsureVisible();

            tvRegistryDirectory.SelectedNode = node;
            node.Expand();
            tvRegistryDirectory.LabelEdit = true;
            node.BeginEdit();
        }

        [PacketHandler(MessageHead.C_NREG_DELETE_KEY_RESPONSE)]
        public void DeleteKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetDeleteRegistryKeyResponsePack>();
            TreeNode parent = GetTreeNode(pack.ParentPath);

            if (parent.Nodes.ContainsKey(pack.KeyName))
            {
                parent.Nodes.RemoveByKey(pack.KeyName);
            }
        }

        [PacketHandler(MessageHead.C_NREG_RENAME_KEY_RESPONSE)]
        public void RenameKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRenameRegistryKeyResponsePack>();
            TreeNode parent = GetTreeNode(pack.ParentPath);

            if (parent.Nodes.ContainsKey(pack.OldKeyName))
            {
                parent.Nodes[pack.OldKeyName].Text = pack.NewKeyName;
                parent.Nodes[pack.OldKeyName].Name = pack.NewKeyName;

                tvRegistryDirectory.SelectedNode = parent.Nodes[pack.NewKeyName];
            }
        }

        /// <summary>
        /// Tries to find the desired TreeNode given the full path to it.
        /// </summary>
        /// <param name="path">The full path to the TreeNode.</param>
        /// <returns>Null if an invalid name is passed or the TreeNode could not be found; The TreeNode represented by the full path.</returns>
        private TreeNode GetTreeNode(string path)
        {
            string[] nodePath = path.Split(new char[] { '\\' });

            TreeNode lastNode = tvRegistryDirectory.Nodes[nodePath[0]];
            if (lastNode == null)
                return null;

            for (int i = 1; i < nodePath.Length; i++)
            {
                lastNode = lastNode.Nodes[nodePath[i]];
                if (lastNode == null)
                    return null;
            }
            return lastNode;
        }

        #endregion

        #region ListView helper functions

        [PacketHandler(MessageHead.C_NREG_CREATE_VALUE_RESPONSE)]
        public void CreateValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetCreateRegistryValueResponsePack>();
            TreeNode key = GetTreeNode(pack.KeyPath);

            if (key != null)
            {
                List<RegValueData> valuesFromNode = ((RegValueData[])key.Tag).ToList();
                valuesFromNode.Add(pack.Value);
                key.Tag = valuesFromNode.ToArray();

                if (tvRegistryDirectory.SelectedNode == key)
                {
                    RegistryValueLstItem item = new RegistryValueLstItem(pack.Value);
                    lstRegistryValues.Items.Add(item);
                    //Unselect all
                    lstRegistryValues.SelectedIndices.Clear();
                    item.Selected = true;
                    lstRegistryValues.LabelEdit = true;
                    item.BeginEdit();
                }

                tvRegistryDirectory.SelectedNode = key;
            }
        }

        [PacketHandler(MessageHead.C_NREG_DELETE_VALUE_RESPONSE)]
        public void DeleteValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetDeleteRegistryValueResponsePack>();
            TreeNode key = GetTreeNode(pack.KeyPath);

            if (key != null)
            {
                if (!RegValueHelper.IsDefaultValue(pack.ValueName))
                {
                    //Remove the values that have the specified name
                    key.Tag = ((RegValueData[])key.Tag).Where(value => value.Name != pack.ValueName).ToArray();

                    if (tvRegistryDirectory.SelectedNode == key)
                        lstRegistryValues.Items.RemoveByKey(pack.ValueName);
                }
                else //Handle delete of default value
                {
                    var regValue = ((RegValueData[])key.Tag).First(item => item.Name == pack.ValueName);

                    if (tvRegistryDirectory.SelectedNode == key)
                    {
                        var valueItem = lstRegistryValues.Items.Cast<RegistryValueLstItem>()
                                                     .SingleOrDefault(item => item.Name == pack.ValueName);
                        if (valueItem != null)
                            valueItem.Data = regValue.Kind.RegistryTypeToString(null);
                    }
                }

                tvRegistryDirectory.SelectedNode = key;
            }
        }

        [PacketHandler(MessageHead.C_NREG_RENAME_VALUE_RESPONSE)]
        public void RenameValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRenameRegistryValueResponsePack>();
            TreeNode key = GetTreeNode(pack.KeyPath);

            if (key != null)
            {
                var value = ((RegValueData[])key.Tag).First(item => item.Name == pack.OldValueName);
                value.Name = pack.NewValueName;

                if (tvRegistryDirectory.SelectedNode == key)
                {
                    var valueItem = lstRegistryValues.Items.Cast<RegistryValueLstItem>()
                                                     .SingleOrDefault(item => item.Name == pack.OldValueName);
                    if (valueItem != null)
                        valueItem.RegName = pack.NewValueName;
                }

                tvRegistryDirectory.SelectedNode = key;
            }
        }

        [PacketHandler(MessageHead.C_NREG_CHANGE_VALUE_RESPONSE)]
        public void ChangeValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetChangeRegistryValueResponsePack>();
            TreeNode key = GetTreeNode(pack.KeyPath);

            if (key != null)
            {
                var regValue = ((RegValueData[])key.Tag).First(item => item.Name == pack.Value.Name);
                ChangeRegistryValue(pack.Value, regValue);

                if (tvRegistryDirectory.SelectedNode == key)
                {
                    var valueItem = lstRegistryValues.Items.Cast<RegistryValueLstItem>()
                                                     .SingleOrDefault(item => item.Name == pack.Value.Name);
                    if (valueItem != null)
                        valueItem.Data = RegValueHelper.RegistryValueToString(pack.Value);
                }

                tvRegistryDirectory.SelectedNode = key;
            }
        }

        private void ChangeRegistryValue(RegValueData source, RegValueData dest)
        {
            if (source.Kind != dest.Kind) return;
            dest.Data = source.Data;
        }

        private void UpdateLstRegistryValues(TreeNode node)
        {
            selectedStripStatusLabel.Text = node.FullPath;

            RegValueData[] ValuesFromNode = (RegValueData[])node.Tag;

            PopulateLstRegistryValues(ValuesFromNode);
        }

        private void PopulateLstRegistryValues(RegValueData[] values)
        {
            lstRegistryValues.BeginUpdate();
            lstRegistryValues.Items.Clear();

            //Sort values
            values = (
                from value in values
                orderby value.Name ascending
                select value
                ).ToArray();

            foreach (var value in values)
            {
                RegistryValueLstItem item = new RegistryValueLstItem(value);
                lstRegistryValues.Items.Add(item);
            }

            lstRegistryValues.EndUpdate();
        }

        #endregion

        #region tvRegistryDirectory actions

        private void tvRegistryDirectory_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                e.CancelEdit = true;

                if (e.Label.Length > 0)
                {
                    if (e.Node.Parent.Nodes.ContainsKey(e.Label))
                    {
                        MessageBox.Show("Invalid label. \nA node with that label already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Node.BeginEdit();
                    }
                    else
                    {
                        _adapter.SendAsyncMessage(MessageHead.S_NREG_RENAME_KEY,
                            new DoRenameRegistryKeyPack()
                            {
                                ParentPath = e.Node.Parent.FullPath,
                                OldKeyName = e.Node.Name,
                                NewKeyName = e.Label
                            });
                        tvRegistryDirectory.LabelEdit = false;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid label. \nThe label cannot be blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Node.BeginEdit();
                }
            }
            else
            {
                //Stop editing if no changes where made
                tvRegistryDirectory.LabelEdit = false;
            }
        }

        private void tvRegistryDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode parentNode = e.Node;

            // If nothing is there (yet).
            if (string.IsNullOrEmpty(parentNode.FirstNode.Name))
            {
                tvRegistryDirectory.SuspendLayout();
                parentNode.Nodes.Clear();

                _adapter.SendAsyncMessage(MessageHead.S_NREG_LOAD_REGKEYS,
                    new DoLoadRegistryKeyPack()
                    {
                        RootKeyName = parentNode.FullPath
                    });

                tvRegistryDirectory.ResumeLayout();

                e.Cancel = true;
            }
        }

        private void tvRegistryDirectory_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //Bug fix with rightclick not working for selectednode
                tvRegistryDirectory.SelectedNode = e.Node;

                //Display the context menu
                Point pos = new Point(e.X, e.Y);
                CreateTreeViewMenuStrip();
                tv_ContextMenuStrip.Show(tvRegistryDirectory, pos);
            }
        }

        private void tvRegistryDirectory_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            UpdateLstRegistryValues(e.Node);
        }

        private void tvRegistryDirectory_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteState())
                deleteRegistryKey_Click(this, e);
        }

        #endregion

        #region ToolStrip and contextmenu helper functions

        private void CreateEditToolStrip()
        {
            this.modifyToolStripMenuItem1.Visible =
                this.modifyBinaryDataToolStripMenuItem1.Visible =
                    this.modifyNewtoolStripSeparator.Visible = lstRegistryValues.Focused;

            this.modifyToolStripMenuItem1.Enabled =
                this.modifyBinaryDataToolStripMenuItem1.Enabled = lstRegistryValues.SelectedItems.Count == 1;

            this.renameToolStripMenuItem2.Enabled = GetRenameState();
            this.deleteToolStripMenuItem2.Enabled = GetDeleteState();
        }

        private void CreateTreeViewMenuStrip()
        {
            this.renameToolStripMenuItem.Enabled = tvRegistryDirectory.SelectedNode.Parent != null;

            this.deleteToolStripMenuItem.Enabled = tvRegistryDirectory.SelectedNode.Parent != null;
        }

        private void CreateListViewMenuStrip()
        {
            this.modifyToolStripMenuItem.Enabled =
                this.modifyBinaryDataToolStripMenuItem.Enabled = lstRegistryValues.SelectedItems.Count == 1;

            this.renameToolStripMenuItem1.Enabled = lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(lstRegistryValues.SelectedItems[0].Name);

            this.deleteToolStripMenuItem1.Enabled = tvRegistryDirectory.SelectedNode != null && lstRegistryValues.SelectedItems.Count > 0;
        }

        #endregion

        #region MenuStrip actions

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            CreateEditToolStrip();
        }

        private void menuStripExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuStripDelete_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.Focused)
            {
                deleteRegistryKey_Click(this, e);
            }
            else if (lstRegistryValues.Focused)
            {
                deleteRegistryValue_Click(this, e);
            }
        }

        private void menuStripRename_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.Focused)
            {
                renameRegistryKey_Click(this, e);
            }
            else if (lstRegistryValues.Focused)
            {
                renameRegistryValue_Click(this, e);
            }
        }

        #endregion

        #region lstRegistryKeys actions

        private void lstRegistryKeys_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pos = new Point(e.X, e.Y);

                //Try to check if a item was clicked
                if (lstRegistryValues.GetItemAt(pos.X, pos.Y) == null)
                {
                    //Not on a item
                    lst_ContextMenuStrip.Show(lstRegistryValues, pos);
                }
                else
                {
                    //Clicked on a item
                    CreateListViewMenuStrip();
                    selectedItem_ContextMenuStrip.Show(lstRegistryValues, pos);
                }
            }
        }

        private void lstRegistryKeys_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null && tvRegistryDirectory.SelectedNode != null)
            {
                e.CancelEdit = true;
                int index = e.Item;

                if (e.Label.Length > 0)
                {
                    if (lstRegistryValues.Items.ContainsKey(e.Label))
                    {
                        MessageBox.Show("Invalid label. \nA node with that label already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        lstRegistryValues.Items[index].BeginEdit();
                        return;
                    }

                    _adapter.SendAsyncMessage(MessageHead.S_NREG_RENAME_VALUE,
                        new DoRenameRegistryValuePack()
                        {
                            KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                            OldValueName = lstRegistryValues.Items[index].Name,
                            NewValueName = e.Label
                        });
                    lstRegistryValues.LabelEdit = false;
                }
                else
                {
                    MessageBox.Show("Invalid label. \nThe label cannot be blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lstRegistryValues.Items[index].BeginEdit();

                }
            }
            else
            {
                lstRegistryValues.LabelEdit = false;
            }
        }

        private void lstRegistryKeys_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && GetDeleteState())
                deleteRegistryValue_Click(this, e);
        }

        #endregion

        #region ContextMenu

        private void createNewRegistryKey_Click(object sender, EventArgs e)
        {
            if (!(tvRegistryDirectory.SelectedNode.IsExpanded) && tvRegistryDirectory.SelectedNode.Nodes.Count > 0)
            {
                //Subscribe (wait for node to expand)
                tvRegistryDirectory.AfterExpand += this.createRegistryKey_AfterExpand;
                tvRegistryDirectory.SelectedNode.Expand();
            }
            else
            {
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_KEY,
                    new DoCreateRegistryKeyPack()
                    {
                        ParentPath = tvRegistryDirectory.SelectedNode.FullPath
                    });
            }
        }

        private void deleteRegistryKey_Click(object sender, EventArgs e)
        {
            // prompt user to confirm delete
            string msg = "Are you sure you want to permanently delete this key and all of its subkeys?";
            string caption = "Confirm Key Delete";
            var answer = MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.Yes)
            {
                string parentPath = tvRegistryDirectory.SelectedNode.Parent.FullPath;

                _adapter.SendAsyncMessage(MessageHead.S_NREG_DELETE_KEY,
                    new DoDeleteRegistryKeyPack()
                    {
                        ParentPath = parentPath,
                        KeyName = tvRegistryDirectory.SelectedNode.Name
                    });
            }
        }

        private void renameRegistryKey_Click(object sender, EventArgs e)
        {
            tvRegistryDirectory.LabelEdit = true;
            tvRegistryDirectory.SelectedNode.BeginEdit();
        }

        #region New registry value actions

        private void createStringRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_SZ
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.String
                    });
            }
        }

        private void createBinaryRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_BINARY
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.Binary
                    });
            }
        }

        private void createDwordRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_DWORD
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.DWord
                    });
            }
        }

        private void createQwordRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_QWORD
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.QWord
                    });
            }
        }

        private void createMultiStringRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_MULTI_SZ
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.MultiString
                    });
            }
        }

        private void createExpandStringRegistryValue_Click(object sender, EventArgs e)
        {
            if (tvRegistryDirectory.SelectedNode != null)
            {
                // request the creation of a new Registry value of type REG_EXPAND_SZ
                _adapter.SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                    new DoCreateRegistryValuePack()
                    {
                        KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                        Kind = RegistryValueKind.ExpandString
                    });
            }
        }

        #endregion

        #region Registry value edit actions

        private void deleteRegistryValue_Click(object sender, EventArgs e)
        {
            //Prompt user to confirm delete
            string msg = "Deleting certain registry values could cause system instability. Are you sure you want to permanently delete " + (lstRegistryValues.SelectedItems.Count == 1 ? "this value?" : "these values?");
            string caption = "Confirm Value Delete";
            var answer = MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.Yes)
            {
                foreach (var item in lstRegistryValues.SelectedItems)
                {
                    if (item.GetType() == typeof(RegistryValueLstItem))
                    {
                        RegistryValueLstItem registryValue = (RegistryValueLstItem)item;
                        _adapter.SendAsyncMessage(MessageHead.S_NREG_DELETE_VALUE,
                            new DoDeleteRegistryValuePack()
                            {
                                KeyPath = tvRegistryDirectory.SelectedNode.FullPath,
                                ValueName = registryValue.RegName
                            });
                    }
                }
            }
        }

        private void renameRegistryValue_Click(object sender, EventArgs e)
        {
            lstRegistryValues.LabelEdit = true;
            lstRegistryValues.SelectedItems[0].BeginEdit();
        }

        private void modifyRegistryValue_Click(object sender, EventArgs e)
        {
            CreateEditForm(false);
        }

        private void modifyBinaryDataRegistryValue_Click(object sender, EventArgs e)
        {
            CreateEditForm(true);
        }

        #endregion

        #endregion

        private void createRegistryKey_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == tvRegistryDirectory.SelectedNode)
            {
                createNewRegistryKey_Click(this, e);

                tvRegistryDirectory.AfterExpand -= createRegistryKey_AfterExpand;
            }
        }

        #region helper functions

        private bool GetDeleteState()
        {
            if (lstRegistryValues.Focused)
                return lstRegistryValues.SelectedItems.Count > 0;
            else if (tvRegistryDirectory.Focused && tvRegistryDirectory.SelectedNode != null)
                return tvRegistryDirectory.SelectedNode.Parent != null;
            return false;
        }

        private bool GetRenameState()
        {
            if (lstRegistryValues.Focused)
                return lstRegistryValues.SelectedItems.Count == 1 && !RegValueHelper.IsDefaultValue(lstRegistryValues.SelectedItems[0].Name);
            else if (tvRegistryDirectory.Focused && tvRegistryDirectory.SelectedNode != null)
                return tvRegistryDirectory.SelectedNode.Parent != null;
            return false;
        }

        private Form GetEditForm(RegValueData value, RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    return new RegValueEditStringForm(value);
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return new RegValueEditWordForm(value);
                case RegistryValueKind.MultiString:
                    return new RegValueEditMultiStringForm(value);
                case RegistryValueKind.Binary:
                    return new RegValueEditBinaryForm(value);
                default:
                    return null;
            }
        }

        private void CreateEditForm(bool isBinary)
        {
            string keyPath = tvRegistryDirectory.SelectedNode.FullPath;
            string name = lstRegistryValues.SelectedItems[0].Name;
            RegValueData value = ((RegValueData[])tvRegistryDirectory.SelectedNode.Tag).ToList().Find(item => item.Name == name);

            // any kind can be edited as binary
            RegistryValueKind kind = isBinary ? RegistryValueKind.Binary : value.Kind;

            using (var frm = GetEditForm(value, kind))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _adapter.SendAsyncMessage(MessageHead.S_NREG_CHANGE_VALUE,
                        new DoChangeRegistryValuePack()
                        {
                            KeyPath = keyPath,
                            Value = (RegValueData)frm.Tag
                        });
                }
            }
        }



        #endregion
    }
}
