using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.Controls
{
    public class AwaitAutoResetEvent
    {
        public byte[] _buffer;
        public AutoResetEvent _event;

        private volatile int _version = 1;//由于task启动线程需要一定时间，因此可能AwaitOneData前，数据就已经返回，导致_event等待死锁
        public AwaitAutoResetEvent(bool initialState)
            => _event = new AutoResetEvent(initialState);

        public void SetOneData(byte[] data = null)
        {
            StackFrame frame = new StackFrame(1, false);
            LogHelper.DebugWriteLog(frame.GetMethod().Name + " SetOneData version:" + _version);
            if (Interlocked.Increment(ref _version) >= 0)
            {
                _buffer = data;
                _event.Set();
            }
        }

        public byte[] AwaitOneData()
        {
            StackFrame frame = new StackFrame(1, false);
            LogHelper.DebugWriteLog(frame.GetMethod().Name + " AwaitOneData version:" + _version);
            var re = Interlocked.Decrement(ref _version);
            LogHelper.DebugWriteLog("re:" + re);
            if (re <= 0)
            {
                LogHelper.DebugWriteLog(frame.GetMethod().Name + " AwaitOneData ----wait version:" + _version);
                _event.Reset();
                _event.WaitOne();
                LogHelper.DebugWriteLog(frame.GetMethod().Name + " AwaitOneData ----wait finish version:" + _version);
            }
            return this._buffer;
        }

        /// <summary>
        /// 重置版本号
        /// </summary>
        public void Reset()
        {
            _version = 1;
        }
    }
}
