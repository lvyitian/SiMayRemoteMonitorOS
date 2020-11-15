using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteControls.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [Rank(100)]
    [ApplicationName("启动项管理")]
    public partial class StartupApplication : Form, IApplication
    {
        [ApplicationAdapterHandler]
        public StartupAdapterHandler StartupAdapterHandler { get; set; }

        private string _title = "//启动管理 #Name#";

        private Dictionary<string, ListViewGroup> _groups = new Dictionary<string, ListViewGroup>();
        public StartupApplication()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Show();
        }

        public void SetParameter(object arg)
        {
            throw new NotImplementedException();
        }

        public void SessionClose(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title + " [" + this.StartupAdapterHandler.State.ToString() + "]";
        }

        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title;
        }
        private async void StartupManager_Load(object sender, EventArgs e)
        {
            this.AddGroups();
            this.Text = this._title = _title.Replace("#Name#", this.StartupAdapterHandler.OriginName);
            var startups = await this.StartupAdapterHandler.GetStartup();
            if (!startups.IsNull())
                OnStartupItemHandlerEvent(startups);
        }

        private void OnStartupItemHandlerEvent(IEnumerable<StartupItemPacket> startupItems)
        {
            lstStartupItems.Items.Clear();

            foreach (var item in startupItems)
            {
                var i = lstStartupItems.Groups
                    .Cast<ListViewGroup>()
                    .First(x => (StartupType)x.Tag == item.Type);
                ListViewItem lvi = new ListViewItem(new[] { item.Name, item.Path })
                {
                    Group = i,
                    Tag = item
                };
                lstStartupItems.Items.Add(lvi);
            }
        }
        /// <summary>
        /// Adds all supported startup types as ListView groups.
        /// </summary>
        private void AddGroups()
        {
            foreach (var startupGroupItem in this.StartupAdapterHandler.StartupGroupItems)
            {
                lstStartupItems.Groups.Add(new ListViewGroup(startupGroupItem.StartupPath)
                {
                    Tag = startupGroupItem.StartupType
                });
            }
        }

        private async void AddEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new StartupItemAdd())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await this.StartupAdapterHandler.AddStartupItem(
                        frm.StartupItem.Path,
                        frm.StartupItem.Name,
                        frm.StartupItem.Type);

                    var startups = await this.StartupAdapterHandler.GetStartup();
                    if (!startups.IsNull())
                        OnStartupItemHandlerEvent(startups);
                }
            }
        }

        private async void RemoveEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var startupItems = new List<StartupItemPacket>();
            foreach (ListViewItem item in lstStartupItems.SelectedItems)
            {
                startupItems.Add(item.Tag as StartupItemPacket);
            }
            await this.StartupAdapterHandler.RemoveStartupItem(startupItems);
            var startups = await this.StartupAdapterHandler.GetStartup();
            if (!startups.IsNull())
                OnStartupItemHandlerEvent(startups);
        }

        private void StartupManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StartupAdapterHandler.CloseSession();
        }
    }
}
