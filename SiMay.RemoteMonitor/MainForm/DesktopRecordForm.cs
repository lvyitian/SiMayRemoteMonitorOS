using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitor.Entitys;
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
    public partial class DesktopRecordForm : Form
    {
        SessionSyncContext _syncContext;

        public DesktopRecordForm(SessionSyncContext syncContext)
        {
            _syncContext = syncContext;
            InitializeComponent();
        }

        private void DesktopRecordManager_Load(object sender, EventArgs e)
        {
            bool isAction = _syncContext.KeyDictions[SysConstants.RecordScreenIsAction].ConvertTo<bool>();
            string macName = _syncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>();
            int screenHeight = _syncContext.KeyDictions[SysConstants.RecordHeight].ConvertTo<int>();
            int screenWidth = _syncContext.KeyDictions[SysConstants.RecordWidth].ConvertTo<int>();
            int spantime = _syncContext.KeyDictions[SysConstants.RecordSpanTime].ConvertTo<int>();

            this.Text += "-" + macName;

            this.tip_label.Text = "该用户的桌面记录文件存储在: " + macName + " 目录";

            this.screenHeightBox.Text = screenHeight.ToString() == "0" ? "800" : screenHeight.ToString(); //默认设置
            this.screenWidthBox.Text = screenWidth.ToString() == "0" ? "1200" : screenWidth.ToString();
            this.spantimeBox.Text = spantime.ToString() == "0" ? "3000" : spantime.ToString();

            if (isAction)
                startbtn.Enabled = false;
            else
                stopbtn.Enabled = false;
        }

        private void startbtn_Click(object sender, EventArgs e)
        {
            int screenHeight = -1;
            int screenWidth = -1;
            int spantime = -1;
            if (!int.TryParse(this.screenHeightBox.Text, out screenHeight) || !int.TryParse(this.screenWidthBox.Text, out screenWidth) || !int.TryParse(spantimeBox.Text, out spantime))
            {
                MessageBoxHelper.ShowBoxExclamation("参数错误,参数只能是数字!");
                return;
            }

            if (spantime < 500)
            {
                MessageBoxHelper.ShowBoxExclamation("记录间隔不能太小");
                return;
            }

            //临时注释
            //byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SCREEN_RECORD_OPEN,
            //    screenHeight.ToString() + "|"
            //    + screenWidth.ToString() + "|"
            //    + spantime.ToString());

            //_session.SendAsync(data);

            _syncContext.KeyDictions[SysConstants.RecordScreenIsAction] = true;
            _syncContext.KeyDictions[SysConstants.RecordHeight] = screenHeight;
            _syncContext.KeyDictions[SysConstants.RecordWidth] = screenWidth;
            _syncContext.KeyDictions[SysConstants.RecordSpanTime] = spantime;

            startbtn.Enabled = false;
            stopbtn.Enabled = true;
        }
        [Obsolete]
        private void stopbtn_Click(object sender, EventArgs e)
        {
            //byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_SCREEN_RECORD_CLOSE);
            //_session.SendAsync(data);

            _syncContext.KeyDictions[SysConstants.RecordScreenIsAction] = false;
            startbtn.Enabled = true;
            stopbtn.Enabled = false;
        }
    }
}
