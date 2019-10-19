using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiMay.Sockets.UtilityHelper
{
    public class LogHelper : IDisposable
    {
        private readonly Queue<string> _que;
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
            _que = new Queue<string>();
            _mre = new AutoResetEvent(false);

            new Thread(new ThreadStart(() =>
            {
                while (_isruning)
                {
                    _mre.WaitOne();
                    try
                    {
                        var sw = new System.IO.StreamWriter("simay.socket_run.log", true);
                        while (_que.Count > 0)
                        {
                            sw.WriteLine("time：{0} -- {1}", DateTime.Now, _que.Dequeue());
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

        public void Dispose()
        {
            _isruning = false;
            _mre.Set();
        }
    }
}
