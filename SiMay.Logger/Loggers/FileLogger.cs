using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SiMay.Logger
{
    public class FileLogger : BaseLogger
    {
        public static string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SiMaylog.log");

        public override void Log(LogLevel level, string log)
        {
            StreamWriter fs = new StreamWriter(fileName, true);
            fs.WriteLine($"{DateTime.Now}-{level}:{log}");
            fs.Close();
        }
    }
}
