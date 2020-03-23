using SiMay.Basic;
using SiMay.RemoteControlsCore;
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
            if (ip.Text == "" || port.Text == "" || connectLimitCount.Text == "" || accessKey.Text == "")
            {
                MessageBoxHelper.ShowBoxExclamation("请正确完整填写设置,否则可能导致客户端上线失败!");
                return;
            }

            AppConfiguration.IPAddress = ip.Text;
            AppConfiguration.Port = int.Parse(port.Text);
            AppConfiguration.MaxConnectCount = int.Parse(connectLimitCount.Text);
            AppConfiguration.DbClickViewExc = (funComboBox.Items[funComboBox.SelectedIndex] as KeyValueItem).Value;
            AppConfiguration.WindowMaximize = maximizeCheckBox.Checked;
            AppConfiguration.LockPassWord = pwdTextBox.Text;
            AppConfiguration.AccessKey = long.Parse(accessKey.Text);
            AppConfiguration.SessionMode = sessionModeList.SelectedIndex.ToString();
            AppConfiguration.ServiceIPAddress = txtservice_address.Text;
            AppConfiguration.ServicePort = int.Parse(txtservice_port.Text);
            AppConfiguration.AccessId = long.Parse(txtAccessId.Text);
            AppConfiguration.EnabledAnonyMous = enableAnonymous.Checked;
            AppConfiguration.MainAppAccessKey = long.Parse(txtMainAppAccessKey.Text);

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
                funComboBox.Text = SysUtil.ApplicationTypes.First(c => !c.IsUnconventionalApp).Type.GetApplicationName();

            ip.Text = AppConfiguration.IPAddress;
            port.Text = AppConfiguration.Port.ToString();
            connectLimitCount.Text = AppConfiguration.MaxConnectCount.ToString();
            pwdTextBox.Text = AppConfiguration.LockPassWord;
            accessKey.Text = AppConfiguration.AccessKey.ToString();
            txtservice_address.Text = AppConfiguration.ServiceIPAddress;
            txtservice_port.Text = AppConfiguration.ServicePort.ToString();
            txtAccessId.Text = AppConfiguration.AccessId.ToString();
            txtMainAppAccessKey.Text = AppConfiguration.MainAppAccessKey.ToString();

            int index = int.Parse(AppConfiguration.SessionMode);
            sessionModeList.SelectedIndex = index;

            maximizeCheckBox.Checked = AppConfiguration.WindowMaximize;

            enableAnonymous.Checked = AppConfiguration.EnabledAnonyMous;
            txtAccessId.Enabled = AppConfiguration.EnabledAnonyMous;

            txtAccessId.Enabled = enableAnonymous.Checked ? false : true;
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

        private void enableAnonymous_CheckedChanged(object sender, EventArgs e)
        {
            txtAccessId.Enabled = enableAnonymous.Checked ? false : true;
        }
    }
}