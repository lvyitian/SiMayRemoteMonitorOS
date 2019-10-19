using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.Daemon
{
    static class Program
    {
        static void Main(string[] args)
        {
            int processId = int.Parse(args[0]);
            string processName = Process.GetProcessById(processId).ProcessName + ".exe";
            while (true)
            {

                try
                {
                    if (Process.GetProcessById(processId).Id != processId)
                        break;
                }
                catch
                {
                    if (File.Exists(processName))
                    {
                        try
                        {
                            Process.Start(processName);
                        }
                        catch { }
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                Thread.Sleep(30000);
            }
        }
    }
}
