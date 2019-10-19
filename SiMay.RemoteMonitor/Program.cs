using SiMay.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm.MainForm());
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ex)
            => WriteException("thread Exception!", ex.ExceptionObject as Exception);

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs ex)
            => WriteException("UI thread Exception!", ex.Exception);

        private static void WriteException(string msg, Exception ex)
        {
            LogHelper.WriteErrorByCurrentMethod(ex);

            if (File.Exists("SiMaylog.log"))
                File.SetAttributes("SiMaylog.log", FileAttributes.Hidden);
        }
    }
}