using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.Startup;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(100, "启动管理", "StartupManagerJob", "", false)]
    public partial class StartupManager : Form, IControlSource
    {
        private string _title = "//启动管理 #Name#";
        private MessageAdapter _adapter;
        private Dictionary<string, ListViewGroup> _groups = new Dictionary<string, ListViewGroup>();
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public StartupManager(MessageAdapter adapter)
        {
            _adapter = adapter;
            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();
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
        private void StartupManager_Load(object sender, EventArgs e)
        {
            this.AddGroups();
            this.Text = _title;
            _adapter.SendAsyncMessage(MessageHead.S_STARTUP_GET_LIST);
        }
        [PacketHandler(MessageHead.C_STARTUP_LIST)]
        public void HandlerStartupItems(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<StartupItemsPack>();

            lstStartupItems.Items.Clear();

            foreach (var item in pack.StartupItems)
            {
                var i = lstStartupItems.Groups.Cast<ListViewGroup>().First(x => (StartupType)x.Tag == item.Type);
                ListViewItem lvi = new ListViewItem(new[] { item.Name, item.Path }) { Group = i, Tag = item };
                lstStartupItems.Items.Add(lvi);
            }
        }
        /// <summary>
        /// Adds all supported startup types as ListView groups.
        /// </summary>
        private void AddGroups()
        {
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = StartupType.LocalMachineRun });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = StartupType.LocalMachineRunOnce });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = StartupType.CurrentUserRun });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = StartupType.CurrentUserRunOnce });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run")
                { Tag = StartupType.LocalMachineWoW64Run });
            lstStartupItems.Groups.Add(
                new ListViewGroup("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
                { Tag = StartupType.LocalMachineWoW64RunOnce });
            lstStartupItems.Groups.Add(
                new ListViewGroup("%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup")
            { Tag = StartupType.StartMenu });
        }

        private void AddEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new StartupItemAdd())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    _adapter.SendAsyncMessage(MessageHead.S_STARTUP_ADD_ITEM, frm.StartupItem);
                }
            }
        }

        private void RemoveEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var startupItems = new List<StartupItemPack>();
            foreach (ListViewItem item in lstStartupItems.SelectedItems)
            {
                startupItems.Add(item.Tag as StartupItemPack);
            }

            _adapter.SendAsyncMessage(MessageHead.S_STARTUP_REMOVE_ITEM, new StartupItemsPack()
            {
                StartupItems = startupItems.ToArray()
            });
        }
    }
}
