using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.Sockets.UtilityHelper
{
    public class LogHelper : IDisposable
    {
        private readonly ConcurrentQueue<string> _que;
        private readonly AutoResetEvent _mre;
        private bool _isruning = true;

        public void WriteLog(string log)
        {
            if (!this._isruning) return;

            _que.Enqueue(log);
            _mre.Set();
        }

        public LogHelper()
        {
            _que = new ConcurrentQueue<string>();
            _mre = new AutoResetEvent(false);

            Task.Factory.StartNew(() => 
            {
                while (_isruning)
                {
                    _mre.WaitOne();

                    try
                    {
                        var sw = new System.IO.StreamWriter("simay.socket_run.log", true);
                        string log;
                        while (_que.Count > 0 && _que.TryDequeue(out log))
                        {
                            sw.WriteLine("time：{0} -- {1}", DateTime.Now, log);
                        }
                        sw.Close();
                    }
                    catch { }
                    Thread.Sleep(1);
                }

                _mre.Dispose();
            }, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            _isruning = false;
            _mre.Set();
        }
    }
}
