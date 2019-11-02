using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class AwaitAutoResetEvent
    {
        public byte[] _buffer;
        public AutoResetEvent _event;

        /// <summary>
        /// 两个方法同步进行
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 防止导致Set比AwaitOne先执行导致_event等待死锁，引入版本号机制
        /// </summary>
        private int _version = 1;
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
                LogHelper.DebugWriteLog(frame.GetMethod().Name + " SetOneData finish version:" + _version);
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
                _event.WaitOne(1000);
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
