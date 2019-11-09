using SiMay.ServiceCore.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SiMay.ServiceCore
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            var desktopName = Win32Interop.GetCurrentDesktop();
            var openProcessString = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            //用户进程启动
            var result = Win32Interop.OpenInteractiveProcess(openProcessString + " \"-user\"", desktopName, true, out _);

        }

        protected override void OnStop()
        {
        }
    }
}
