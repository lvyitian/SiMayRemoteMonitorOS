using SiMay.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.HttpRemoteMonitorService
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            try
            {
                new HttpRemoteMonitorService();
            }
            catch (Exception ex)
            {
                ExceptionProcess(ex);
            }
            

            Application.Run();
        }

        static string logPath = "SiMayWebRemoteMonitorServicelog.log";
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            ExceptionProcess(ex);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            ExceptionProcess(ex);
        }

        private static void ExceptionProcess(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.Message);
            sb.Append(ex.StackTrace);

            LogHelper.WriteErrorByCurrentMethod(sb.ToString());

            if (File.Exists(logPath))
                File.SetAttributes(logPath, FileAttributes.Hidden);
        }
    }
}
