using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiMay.Sockets.UtilityHelper
{
    public class LogHelper
    {
        private readonly static ConcurrentQueue<string> _que;
        private readonly static AutoResetEvent _mre;
        private static bool _whetherRuning = true;
        public static void WriteLog(string log)
        {
            if (!_whetherRuning)
                return;
            Console.WriteLine(log);

            _que.Enqueue(log);
            _mre.Set();

        }

        static LogHelper()
        {
            _que = new ConcurrentQueue<string>();
            _mre = new AutoResetEvent(false);

            new Thread(new ThreadStart(() =>
            {
                while (_whetherRuning)
                {
                    _mre.WaitOne();
                    try
                    {
                        var sw = new System.IO.StreamWriter("simay.socket_run.log", true);
                        while (_que.Count > 0)
                        {
                            var log = string.Empty;
                            if (_que.TryDequeue(out log))
                                sw.WriteLine("time：{0} -- {1}", DateTime.Now, log);
                        }
                        sw.Close();
                    }
                    catch { }
                    Thread.Sleep(1);
                }

                _mre.Close();
            }))
            { IsBackground = true }.Start();
        }

        public static void DisposeLogThread()
        {
            _whetherRuning = false;
            _mre.Set();
        }
    }
}
