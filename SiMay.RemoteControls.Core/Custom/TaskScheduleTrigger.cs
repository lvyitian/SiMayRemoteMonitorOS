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
            Task.Factory.StartNew(async () =>
            {
                while (_stared)
                {
                    var now = DateTime.Now;
                    ITaskSchedule[] toLists = Array.Empty<ITaskSchedule>();
                    lock (_taskSchedules)
                        toLists = _taskSchedules.ToArray();//临时解决方案

                    foreach (var task in toLists)
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
                    Array.Clear(toLists, 0, toLists.Length);
                    await Task.Delay(intervalMin);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void AddScheduleTask(ITaskSchedule task)
        {
            task.TimePoint = DateTime.Now;
            lock (_taskSchedules)
                _taskSchedules.Add(task);
        }
        public static bool FindOutScheduleTask(Func<ITaskSchedule, bool> func, out ITaskSchedule task)
        {
            lock (_taskSchedules)
                task = _taskSchedules.FirstOrDefault(func);

            return !task.IsNull();
        }

        public static void RemoveScheduleTask(ITaskSchedule task)
        {
            lock (_taskSchedules)
                _taskSchedules.Remove(task);
        }
    }
}
