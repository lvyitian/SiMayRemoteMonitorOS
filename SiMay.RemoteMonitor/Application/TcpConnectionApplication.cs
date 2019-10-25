using SiMay.Core.Packets.TcpConnection;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [ApplicationName("Tcp连接管理")]
    [Application(typeof(TcpConnectionAdapterHandler), "TcpConnectionManagerJob", 90)]
    public partial class TcpConnectionApplication : Form, IApplication
    {
        [ApplicationAdapterHandler]
        public TcpConnectionAdapterHandler TcpConnectionAdapterHandler { get; set; }

        private string _title = "//Tcp连接管理 #Name#";
        private Dictionary<string, ListViewGroup> _groups = new Dictionary<string, ListViewGroup>();

        /// <summary>
        /// 构造函数必须为(MessageAdapter adapter)，否则将引发异常
        /// </summary>
        /// <param name="adapter"></param>
        public TcpConnectionApplication()
        {
            InitializeComponent();
        }

        public void Start()
            => this.Show();

        public void SessionClose(AdapterHandlerBase handler)
            => this.Text = _title + " [" + handler.StateContext.ToString() + "]";

        public void ContinueTask(AdapterHandlerBase handler)
            => this.Text = _title;

        private void TcpConnectionManager_Load(object sender, EventArgs e)
        {
            this.Text = _title = _title.Replace("#Name#", TcpConnectionAdapterHandler.OriginName);
            TcpConnectionAdapterHandler.OnTcpListHandlerEvent += OnTcpListHandlerEvent;
            TcpConnectionAdapterHandler.GetTcpList();
        }

        private void OnTcpListHandlerEvent(TcpConnectionAdapterHandler adapterHandler, IEnumerable<TcpConnectionItem> tcplist)
        {
            lstConnections.Items.Clear();

            foreach (var con in tcplist)
            {
                string state = con.State.ToString();

                ListViewItem lvi = new ListViewItem(new[]
                {
                    con.ProcessName,
                    con.LocalAddress,
                    con.LocalPort.ToString(),
                    con.RemoteAddress,
                    con.RemotePort.ToString(),
                    state
                });

                if (!_groups.ContainsKey(state))
                {
                    // create new group if not exists already
                    ListViewGroup g = new ListViewGroup(state, state);
                    lstConnections.Groups.Add(g);
                    _groups.Add(state, g);
                }

                lvi.Group = lstConnections.Groups[state];
                lstConnections.Items.Add(lvi);
            }
        }
        /// <summary>
        /// 关闭窗口前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            TcpConnectionAdapterHandler.OnTcpListHandlerEvent -= OnTcpListHandlerEvent;
            TcpConnectionAdapterHandler.CloseHandler();
        }
        private void 刷新列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TcpConnectionAdapterHandler.GetTcpList();
        }

        private void 关闭连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<KillTcpConnectionItem> kills = new List<KillTcpConnectionItem>();
            foreach (ListViewItem lvi in lstConnections.SelectedItems)
            {
                kills.Add(new KillTcpConnectionItem()
                {
                    LocalAddress = lvi.SubItems[1].Text,
                    LocalPort = lvi.SubItems[2].Text,
                    RemoteAddress = lvi.SubItems[3].Text,
                    RemotePort = lvi.SubItems[4].Text
                });
            }
            TcpConnectionAdapterHandler.CloseTcpList(kills);
        }
    }
}
