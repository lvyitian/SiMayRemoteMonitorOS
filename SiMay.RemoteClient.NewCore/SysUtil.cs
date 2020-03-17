using SiMay.Basic;
using SiMay.ServiceCore.Extensions;
using SiMay.ServiceCore.MainService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SiMay.ServiceCore
{
    public class SysUtil
    {
        public class ServiceTypeItem
        {
            public string ServiceKey { get; set; }
            public Type AppServiceType { get; set; }
        }
        public static List<ServiceTypeItem> ControlTypes { get; set; }
        static SysUtil()
        {
            ControlTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(type => typeof(ApplicationRemoteService).IsAssignableFrom(type) && type.IsSubclassOf(typeof(ApplicationRemoteService)) && type.IsClass)
                .Select(type => new ServiceTypeItem()
                {
                    ServiceKey = type.GetServiceKey() ?? throw new Exception(type.Name + ":The serviceKey cannot be empty!"),
                    AppServiceType = type
                })
                .ToList();
        }
    }

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
                    SystemSessionHelper.SetSessionStatus(SystemSessionHelper.UNSTALL);//退出
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
