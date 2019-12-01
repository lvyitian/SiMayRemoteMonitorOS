using SiMay.Basic;
using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitor.Entitys;
using SiMay.RemoteMonitor.Extensions;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainApplication
{

    public partial class AppSettingForm : Form
    {

        public AppSettingForm()
        {
            InitializeComponent();
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (ip.Text == "" || port.Text == "" || connectNum.Text == "" || conPwd.Text == "")
            {
                MessageBoxHelper.ShowBoxExclamation("请正确完整填写设置,否则可能导致客户端上线失败!");
                return;
            }

            AppConfiguration.IPAddress = ip.Text;
            AppConfiguration.Port = port.Text;
            AppConfiguration.MaxConnectCount = connectNum.Text;
            AppConfiguration.ConnectPassWord = conPwd.Text;
            AppConfiguration.DbClickViewExc = (funComboBox.Items[funComboBox.SelectedIndex] as KeyValueItem).Value;
            AppConfiguration.WindowMaximize = maximizeCheckBox.Checked.ToString();
            AppConfiguration.LockPassWord = pwdTextBox.Text;
            AppConfiguration.AccessKey = accessKey.Text;
            AppConfiguration.SessionMode = sessionModeList.SelectedIndex.ToString();
            AppConfiguration.ServiceIPAddress = txtservice_address.Text;
            AppConfiguration.ServicePort = txtservice_port.Text;

            DialogResult result = MessageBox.Show("设置完成，是否重启生效设置?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

            if (result == DialogResult.OK)
            {
                System.Windows.Forms.Application.Restart();
            }
        }

        private void SetForm_Load(object sender, EventArgs e)
        {
            SysUtil.ApplicationTypes.ForEach(c =>
            {
                var type = c.Type;
                funComboBox.Items.Add(new KeyValueItem()
                {
                    Key = type.GetApplicationName(),
                    Value = c.ApplicationKey
                });
                if (c.ApplicationKey == AppConfiguration.DbClickViewExc)
                    funComboBox.Text = type.GetApplicationName();
            });

            if (funComboBox.SelectedIndex == -1)
                funComboBox.Text = SysUtil.ApplicationTypes.First().Type.GetApplicationName();

            ip.Text = AppConfiguration.IPAddress;
            conPwd.Text = AppConfiguration.ConnectPassWord;
            port.Text = AppConfiguration.Port;
            connectNum.Text = AppConfiguration.MaxConnectCount;
            pwdTextBox.Text = AppConfiguration.LockPassWord;
            accessKey.Text = AppConfiguration.AccessKey;
            txtservice_address.Text = AppConfiguration.ServiceIPAddress;
            txtservice_port.Text = AppConfiguration.ServicePort;

            int index = int.Parse(AppConfiguration.SessionMode);
            sessionModeList.SelectedIndex = index;

            if (Boolean.Parse(AppConfiguration.WindowMaximize))
                maximizeCheckBox.Checked = true;
            else
                maximizeCheckBox.Checked = false;
        }

        private void conPwd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}