using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Platform.Windows.Helper
{
    public class SystemMessageNotify
    {
        static NotifyIcon notifyIcon = new NotifyIcon();

        public static void InitializeNotifyIcon()
        {
            ContextMenuStrip contextMenus = new ContextMenuStrip();

            var exitMenu = new ToolStripMenuItem("退出服务");
            exitMenu.MouseDown += (s, e) =>
            {
                if (MessageBox.Show("确认退出服务吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    SystemSessionHelper.UnInstall();
            };
            var aboutMenu = new ToolStripMenuItem("关于服务");
            aboutMenu.MouseDown += (s, e) =>
            {
                MessageBox.Show("SiMay远程管理是一款开源的Windows系统远程协助系统，支持远程桌面、文件管理、远程语音、远程摄像头、远程注册表、远程shell等功能，您当前运行的是被控服务程序。", "关于程序", 0, MessageBoxIcon.Exclamation);
            };
            contextMenus.Items.Add(aboutMenu);
            contextMenus.Items.Add(exitMenu);

            notifyIcon.Text = "SiMay被控服务正在运行中...";
            notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            notifyIcon.ContextMenuStrip = contextMenus;
            notifyIcon.Visible = true;
        }

        public static void ShowTip(string message)
        {
            ShowTip(2000, "SiMay被控服务提示", message);
        }
        public static void ShowTip(int time, string title, string message)
        {
            ShowTip(time, title, message, ToolTipIcon.Info);
        }
        public static void ShowTip(int time, string title, string message, ToolTipIcon tipIcon)
        {
            notifyIcon.ShowBalloonTip(time, title, message, tipIcon);
        }
    }
}
