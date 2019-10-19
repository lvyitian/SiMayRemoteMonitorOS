using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.SessionProviderService
{
    public partial class AppOptionsDialog : Form
    {
        public AppOptionsDialog()
        {
            InitializeComponent();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (txtIPAddress.Text == "" || txtPort.Text == "" || txtBacklog.Text == "" || txtAccessKey.Text == "")
            {
                MessageBox.Show("请完整输入服务器配置信息", "提示", 0, MessageBoxIcon.Exclamation);
                return;
            }
            ApplicationConfiguration.IPAddress = txtIPAddress.Text;
            ApplicationConfiguration.Port = txtPort.Text;
            ApplicationConfiguration.Backlog = txtBacklog.Text;
            ApplicationConfiguration.ServiceAccessKey = txtAccessKey.Text;

            AccessKeyExamine.ServiceAccessKey = long.Parse(txtAccessKey.Text);

            DialogResult result = MessageBox.Show("设置已保存,是否重启程序以生效已保存的应用设置?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

            if (result == DialogResult.OK)
            {
                Application.Restart();
            }
        }

        private void AppOptions_Load(object sender, EventArgs e)
        {
            txtIPAddress.Text = ApplicationConfiguration.IPAddress;
            txtPort.Text = ApplicationConfiguration.Port;
            txtBacklog.Text = ApplicationConfiguration.Backlog;
            txtAccessKey.Text = ApplicationConfiguration.ServiceAccessKey;
        }
    }
}
