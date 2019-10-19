using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiMay.Basic
{
    public class ThreadHelper
    {
        public static void CreateThread(ThreadStart threadStart, bool isBackground)
        {
            Thread thread = new Thread(threadStart);
            thread.IsBackground = isBackground;
            thread.Start();
        }

        public static void ThreadPoolStart(WaitCallback waitCallback, object state = null)
        {
            ThreadPool.QueueUserWorkItem(waitCallback, state);
        }
    }
}
