using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public class TaskScheduleTrigger
    {
        private static bool _stared = false;

        private static IList<ITaskSchedule> _taskSchedules;

        //待移除队列
        private static Queue<ITaskSchedule> _waitRemoveSchedules;

        private static Queue<ITaskSchedule> _waitIncreaseSchedules;

        private static int _intervalMin = 100;

        /// <summary>
        /// 启动调度器
        /// </summary>
        /// <param name="intervalMin">最小调度间隔</param>
        public static void StarSchedule(int intervalMin)
        {
            _stared = true;
            _intervalMin = intervalMin;
            _taskSchedules = new List<ITaskSchedule>();
            _waitRemoveSchedules = new Queue<ITaskSchedule>();
            _waitIncreaseSchedules = new Queue<ITaskSchedule>();
            Task.Factory.StartNew(async () =>
            {
                while (_stared)
                {
                    var now = DateTime.Now;
                    foreach (var task in _taskSchedules)
                    {
                        if ((now - task.TimePoint) > task.Interval)
                        {
                            await Task.Run(() =>
                            {
                                task.TimePoint = now;
                                task.Execute();
                            });
                        }
                    }
                    if (_waitRemoveSchedules.Count > 0)
                        _taskSchedules.Remove(_waitRemoveSchedules.Dequeue());
                    if (_waitIncreaseSchedules.Count > 0)
                        _taskSchedules.Add(_waitIncreaseSchedules.Dequeue());

                    await Task.Delay(intervalMin);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void AddScheduleTask(ITaskSchedule task)
        {
            task.TimePoint = DateTime.Now;
            _waitIncreaseSchedules.Enqueue(task);
        }
        public static bool FindOutScheduleTask(Func<ITaskSchedule, bool> func, out ITaskSchedule task)
        {
            task = _taskSchedules.FirstOrDefault(func);
            if (task.IsNull())
                task = _waitIncreaseSchedules.FristOrDefault(func);

            return !task.IsNull();
        }

        public static void RemoveScheduleTask(ITaskSchedule task)
        {
            _waitRemoveSchedules.Enqueue(task);
            //_taskSchedules.Remove(task);
        }
    }
}
