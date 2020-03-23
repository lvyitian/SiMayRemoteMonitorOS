using SiMay.Core;
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
    public partial class ScreenMonitorChangeForm : Form
    {
        public ScreenMonitorChangeForm()
        {
            InitializeComponent();
        }

        public int MonitorCount
        {
            get
            {
                return _monitorCount;
            }
        }

        public int CurrentMonitorIndex
        {
            get
            {
                return _currentMonitorIndex;
            }
        }

        public void SetMonitors(MonitorItem[] monitorItems, int screenIndex)
        {
            this.monitorsCombox.Items.Clear();
            foreach (var monitor in monitorItems)
                this.monitorsCombox.Items.Add(monitor.DeviceName + (monitor.Primary ? "[主显示器]" : "(扩展)"));

            this.monitorsCombox.Text = this.monitorsCombox.Items[screenIndex].ToString();
            _monitorCount = monitorItems.Length;
        }

        private int _currentMonitorIndex = 0;
        private int _monitorCount = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            this._currentMonitorIndex = this.monitorsCombox.SelectedIndex;
            this.DialogResult = DialogResult.OK;
        }
    }
}
