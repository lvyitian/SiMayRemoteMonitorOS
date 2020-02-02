using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.Packets.Startup;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
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
    [ApplicationName("启动项管理")]
    [Application(typeof(StartupAdapterHandler), AppJobConstant.REMOTE_STARTUP, 100)]
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

        public void SessionClose(ApplicationAdapterHandler handler)
        {
            this.Text = _title + " [" + this.StartupAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            this.Text = _title;
        }
        private void StartupManager_Load(object sender, EventArgs e)
        {
            this.AddGroups();
            this.Text = this._title = _title.Replace("#Name#", this.StartupAdapterHandler.OriginName);
            this.StartupAdapterHandler.OnStartupItemHandlerEvent += OnStartupItemHandlerEvent;
            this.StartupAdapterHandler.GetStartup();
        }

        private void OnStartupItemHandlerEvent(StartupAdapterHandler adapterHandler, IEnumerable<StartupItemPack> startupItems)
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
                lstStartupItems.Groups.Add(
                    new ListViewGroup(startupGroupItem.StartupPath)
                    { Tag = startupGroupItem.StartupType });
            }
        }

        private void AddEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new StartupItemAdd())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    this.StartupAdapterHandler.AddStartupItem(
                        frm.StartupItem.Path,
                        frm.StartupItem.Name,
                        frm.StartupItem.Type);
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
            this.StartupAdapterHandler.RemoveStartupItem(startupItems);
        }

        private void StartupManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StartupAdapterHandler.OnStartupItemHandlerEvent -= OnStartupItemHandlerEvent;
            this.StartupAdapterHandler.CloseSession();
        }
    }
}
