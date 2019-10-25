using SiMay.ServiceCore.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore.MainService
{
    /// <summary>
    /// 线程安全的任务队列，防止任务重复创建
    /// </summary>
    public class ManagerTaskQueue : Queue<ServiceManager>
    {
        private readonly object _taskopLock = new object();

        public new void Enqueue(ServiceManager manager)
        {
            lock (_taskopLock)
            {
                base.Enqueue(manager);
            }
        }
        public new ServiceManager Dequeue()
        {
            ServiceManager manager = null;
            lock (_taskopLock)
            {
                if (this.Count > 0)
                    manager = base.Dequeue();
            }
            return manager;
        }
    }
}
