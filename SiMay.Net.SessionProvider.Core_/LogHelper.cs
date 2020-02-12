using System;
using System.IO;

namespace SiMay.Net.SessionProvider.Core
{

    public class LogHelper
    {

        public static void WriteLog(string log, string LogAddress = "")
        {
            if (LogAddress == "")
                LogAddress = Environment.CurrentDirectory + @"\SiMaylog.log";

            try
            {
                StreamWriter fs = new StreamWriter(LogAddress, true);
                fs.WriteLine(DateTime.Now.ToString() + " " + log);
                fs.Close();
            }
            catch { }

        }
    }
}