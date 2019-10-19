using SiMay.Basic;
using SiMay.RemoteControlsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainForm
{
    public partial class LockWindowsForm : Form
    {
        public LockWindowsForm()
        {
            InitializeComponent();
        }

        bool _ifree = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (pwdTextBox.Text.Equals(AppConfiguration.LockPassWord))
            {
                this._ifree = true;
                AppConfiguration.WindowsIsLock = "false";
                this.Close();
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("解锁密码错误~");
            }
        }

        private void LockWindows_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_ifree)
            {
                e.Cancel = true;
                MessageBoxHelper.ShowBoxExclamation("请输入密码进行解锁哦~");
            }
        }
    }
}
