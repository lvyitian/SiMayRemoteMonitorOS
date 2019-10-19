using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.UpdateClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            int mainProcessId = int.Parse(args[0]);
            int deamonProcessId = int.Parse(args[1]);

            string fileName = args[2].Replace("|", " ") + ".exe";

            //关闭守护进程，防止更新时守护进程检测到主程序已关闭重启导致的更新失败!
            Process deamonProcess = Process.GetProcessById(deamonProcessId);
            deamonProcess.Kill();
            //退出客户端程序
            Process process = Process.GetProcessById(mainProcessId);
            process.Kill();

            //确保客户端完全释放
            Thread.Sleep(3000);

            //删除主程序，替换新程序
            File.Delete(fileName);

            if (File.Exists("updateF"))
                File.Move("updateF", fileName);

            Process.Start(fileName);

            //更新完成程序退出
        }
    }
}
