using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiMay.ServiceCore.ApplicationService
{
    public class ProcessSpy
    {
        Dictionary<int, Process> _processs = new Dictionary<int, Process>();
        bool _isRun;
        public void StartDiff()
        {
            Thread _thread = new Thread(() =>
            {
                while (this._isRun)
                {
                    var process = Process.GetProcesses();


                }
            });
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void ExitDiff()
        {

        }
    }

    public enum ChangeType
    {
        /// <summary>
        /// 新建进程信息
        /// </summary>
        CreateProcess,
        /// <summary>
        /// 更新进程信息
        /// </summary>
        UpdateProcess,
        /// <summary>
        /// 移除进程
        /// </summary>
        RemoveProcess
    }
}
