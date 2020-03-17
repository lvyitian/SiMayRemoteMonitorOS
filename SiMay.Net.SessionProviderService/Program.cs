using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.SessionProviderService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SessionProviderService());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            Exception e = (Exception)ex.ExceptionObject;
            LogHelper.WriteLog("子线程异常:" + Environment.NewLine +
                "异常信息:" + e.Message + Environment.NewLine +
                "异常堆栈:" + e.StackTrace + Environment.NewLine, ApplicationConfiguration.LogFileName);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs ex)
        {
            Exception e = ex.Exception;

            LogHelper.WriteLog("UI异常:" + Environment.NewLine +
                "异常信息:" + e.Message + Environment.NewLine +
                "异常堆栈:" + e.StackTrace + Environment.NewLine, ApplicationConfiguration.LogFileName);
        }
    }
}
