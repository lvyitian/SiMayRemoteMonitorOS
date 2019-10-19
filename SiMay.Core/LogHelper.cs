using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace SiMay.Core
{

    public class LogHelper : IDisposable
    {
        public static string fileName = Environment.CurrentDirectory + @"\SiMaylog.log";

        static bool _isDisposed = false;
        static object _lock = new object();
        static Queue<string> _logQueue = new Queue<string>();
        static ManualResetEvent _event = new ManualResetEvent(false);
        static LogHelper()
        {
            ThreadHelper.CreateThread(() =>
            {
                while (!_isDisposed)
                {
                    _event.WaitOne();
                    lock (_lock)
                    {
                        var logs = _logQueue.ToArray();
                        _logQueue.Clear();
                        if (!logs.IsNullOrEmpty())
                            Write(logs, fileName);
                    }
                    _event.Reset();
                    Thread.Sleep(100);//缓存一下
                }
            }, true);
        }

        public static void WriteErrorByCurrentMethod(Exception ex)
        {
            StackFrame frame = new StackFrame(1, false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DateTime:" + DateTime.Now.ToString());
            sb.AppendLine("Method:" + frame.GetMethod().Name);
            sb.AppendLine("Exception Message:" + ex.Message);
            sb.AppendLine("StackTrace:" + ex.StackTrace);
            _logQueue.Enqueue(sb.ToString());
            _event.Set();
        }

        public static void WriteErrorByCurrentMethod(string log)
        {
            StackFrame frame = new StackFrame(1, false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DateTime:" + DateTime.Now.ToString());
            sb.AppendLine("Method:" + frame.GetMethod().Name);
            sb.AppendLine("Log:" + log);
            _logQueue.Enqueue(sb.ToString());
            _event.Set();
        }

        public static void WriteLog(string log, string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DateTime:" + DateTime.Now.ToString());
            sb.AppendLine("Log:" + log);
            Write(new string[] { sb.ToString() }, path);
        }

        public static void DebugWriteLog(string log)
        {
#if DEBUG
            log = "DateTime:" + DateTime.Now.ToString() + " - " + log;
            Console.WriteLine(log);
            _logQueue.Enqueue(log);
            _event.Set();
#endif
        }

        public static void Write(string[] logs, string path)
        {
            try
            {
                StreamWriter fs = new StreamWriter(path, true);
                foreach (var log in logs)
                    fs.WriteLine(log);
                fs.Close();
            }
            catch { }

        }

        public void Dispose()
        {
            _isDisposed = true;
            _event.Set();
        }
    }
}