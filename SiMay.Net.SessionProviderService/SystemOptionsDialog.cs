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
    public partial class SystemOptionsDialog : Form
    {
        public SystemOptionsDialog()
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
            ApplicationConfiguration.LoalAddress = txtIPAddress.Text;
            ApplicationConfiguration.Port = int.Parse(txtPort.Text);
            ApplicationConfiguration.Backlog = int.Parse(txtBacklog.Text);
            ApplicationConfiguration.AccessKey = long.Parse(txtAccessKey.Text);
            ApplicationConfiguration.AnonyMous = this.ckAnonyMous.Checked;
            ApplicationConfiguration.AccessIds = txtAccessId.Text;
            ApplicationConfiguration.MainAppAccessKey = long.Parse(txtMainAppAccessKey.Text);

            DialogResult result = MessageBox.Show("设置已保存,是否重启程序以生效已保存的应用设置?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

            if (result == DialogResult.OK)
            {
                Application.Restart();
            }
        }

        private void AppOptions_Load(object sender, EventArgs e)
        {
            txtIPAddress.Text = ApplicationConfiguration.LoalAddress;
            txtPort.Text = ApplicationConfiguration.Port.ToString();
            txtBacklog.Text = ApplicationConfiguration.Backlog.ToString();
            txtAccessKey.Text = ApplicationConfiguration.AccessKey.ToString();
            txtAccessId.Text = ApplicationConfiguration.AccessIds;
            txtMainAppAccessKey.Text = ApplicationConfiguration.MainAppAccessKey.ToString();
            ckAnonyMous.Checked = ApplicationConfiguration.AnonyMous;

            ckAnonyMous.Text = ckAnonyMous.Checked ? "允许" : "不允许";
            checkBox1.Text = checkBox1.Checked ? "开机启动" : "不启动";
        }

        private void ckAnonyMous_CheckedChanged(object sender, EventArgs e)
        {
            ckAnonyMous.Text = ckAnonyMous.Checked ? "允许" : "不允许";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Text = checkBox1.Checked ? "开机启动" : "不启动";
        }
    }
}
