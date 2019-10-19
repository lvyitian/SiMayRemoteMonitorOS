using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class Log
    {
        public int Success { get; set; }

        public string log { get; set; }
    }
    public class LogShowQueueHelper
    {
        public static Queue<Log> LogQueue = new Queue<Log>();

        public static void WriteLog(string log, string status = "ok")
        {
            LogQueue.Enqueue(new Log()
            {
                Success = status == "ok" ? 0 : 1,
                log = log
            });
        }
    }
}
