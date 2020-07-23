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
        private static int _intervalMin = 100;

        public static void StarSchedule(int intervalMin)
        {
            _stared = true;
            _intervalMin = intervalMin;
            _taskSchedules = new List<ITaskSchedule>();
            _waitRemoveSchedules = new Queue<ITaskSchedule>();
            ThreadHelper.CreateThread(async () =>
            {
                while (_stared)
                {
                    var now = DateTime.Now;
                    foreach (var task in _taskSchedules)
                    {
                        await Task.Run(() =>
                        {
                            if ((now - task.TimePoint) > task.Interval)
                            {
                                task.TimePoint = now;
                                task.Execute();
                            }
                        });

                    }
                    if (_waitRemoveSchedules.Count > 0)
                        _taskSchedules.Remove(_waitRemoveSchedules.Dequeue());
                    await Task.Delay(intervalMin);
                }
            }, true);
        }

        public static void AddScheduleTask(ITaskSchedule task)
        {
            task.TimePoint = DateTime.Now;
            _taskSchedules.Add(task);
        }
        public static bool FindScheduleTask(Func<ITaskSchedule, bool> func, out ITaskSchedule task)
        {
            task = _taskSchedules.FirstOrDefault(func);
            return !task.IsNull();
        }

        public static void RemoveScheduleTask(ITaskSchedule task)
        {
            _waitRemoveSchedules.Enqueue(task);
            //_taskSchedules.Remove(task);
        }
    }
}
