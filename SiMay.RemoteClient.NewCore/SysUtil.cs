using SiMay.Basic;
using SiMay.ServiceCore.ApplicationService;
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
        public class ServiceTypeContext
        {
            public string ServiceKey { get; set; }
            public Type AppServiceType { get; set; }
        }
        public static List<ServiceTypeContext> ControlTypes { get; set; }
        static SysUtil()
        {
            List<ServiceTypeContext> controlTypes = new List<ServiceTypeContext>();
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if (typeof(ServiceManagerBase).IsAssignableFrom(type) && type.IsSubclassOf(typeof(ServiceManagerBase)) && type.IsClass)
                {
                    var context = new ServiceTypeContext()
                    {
                        ServiceKey = type.GetServiceKey() ?? throw new Exception(type.Name + ":The serviceKey cannot be empty!"),
                        AppServiceType = type
                    };
                    controlTypes.Add(context);
                }
            }
            ControlTypes = controlTypes;
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
                    ComputerSessionHelper.SessionManager(ComputerSessionHelper.UNSTALL);//退出
            };
            var aboutMenu = new ToolStripMenuItem("关于服务");
            aboutMenu.MouseDown += (s, e) =>
            {
                MessageBoxHelper.ShowBoxExclamation("SiMay远程管理是一款开源的Windows系统远程协助系统，支持远程桌面、文件管理、远程语音、远程摄像头、远程注册表、远程shell等功能，您当前运行的是被控服务程序。", "关于程序");
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
