using SiMay.Basic;
using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public interface ITaskSchedule
    {
        /// <summary>
        /// 任务表示
        /// </summary>
        string TaskName { get; set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        TimeSpan Interval { get; set; }

        /// <summary>
        /// 最后执行时间
        /// </summary>
        DateTime TimePoint { get; set; }

        /// <summary>
        /// 定时执行
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// 自定义调用事件
    /// </summary>
    public interface ICustomEvent
    {
        /// <summary>
        /// 自定义调用
        /// </summary>
        /// <param name="eventArgs"></param>
        void Invoke(object sender, EventArgs eventArgs);
    }

    public abstract class TaskScheduleContextBase : ITaskSchedule
    {
        public string TaskName { get; set; }

        public virtual TimeSpan Interval { get; set; }

        public DateTime TimePoint { get; set; }

        public abstract void Execute();
    }

    public class SuspendTaskContext : TaskScheduleContextBase, ICustomEvent
    {
        /// <summary>
        /// 间隔2秒执行一次
        /// </summary>
        public override TimeSpan Interval { get; set; } = new TimeSpan(0, 0, 2);
        /// <summary>
        /// 离线时间
        /// </summary>
        public DateTime DisconnectTimePoint { get; set; }

        public IList<SessionSyncContext> SessionSyncContexts { get; set; }

        public ApplicationAdapterHandler ApplicationAdapterHandler { get; set; }

        public override void Execute()
        {
            Console.WriteLine(DateTime.Now);
            if ((DateTime.Now - DisconnectTimePoint).Seconds > 5)
            {
                string id = ApplicationAdapterHandler.IdentifyId.Split('|')[0];
                var syncContext = SessionSyncContexts.FirstOrDefault(x => x[SysConstants.IdentifyId].ConvertTo<string>() == id);

                //LogHelper.WriteErrorByCurrentMethod("beigin Reset--{0},{1},{2}".FormatTo(ApplicationAdapterHandler.GetApplicationKey(), ApplicationAdapterHandler.IdentifyId, id));

                if (!syncContext.IsNull())
                {
                    if (ApplicationAdapterHandler.IsManualClose())
                    {
                        //窗口关闭将不再建立连接
                        TaskScheduleTrigger.RemoveScheduleTask(this);
                        return;
                    }

                    var appName = ApplicationAdapterHandler.App.GetType().Name;
                    syncContext.Session.SendTo(MessageHead.S_MAIN_ACTIVATE_APPLICATION_SERVICE,
                        new ActivateServicePack()
                        {
                            CommandText = $"{appName}.{ApplicationAdapterHandler.GetApplicationKey()}"
                        });

                    //LogHelper.WriteErrorByCurrentMethod("send reset command--{0},{1},{2}".FormatTo(ApplicationAdapterHandler.GetApplicationKey(), ApplicationAdapterHandler.IdentifyId, id));
                }
            }
        }

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="eventArgs"></param>
        public void Invoke(object sender, EventArgs eventArgs)
        {
            var taskResumEventArg = eventArgs as SuspendTaskResumEventArgs;
            var tokens = taskResumEventArg.Session.AppTokens;
            tokens[SysConstants.INDEX_WORKTYPE] = ConnectionWorkType.WORKCON;
            tokens[SysConstants.INDEX_WORKER] = ApplicationAdapterHandler;
            ApplicationAdapterHandler.OriginName = taskResumEventArg.OriginName;
            ApplicationAdapterHandler.SetSession(taskResumEventArg.Session);
            ApplicationAdapterHandler.ContinueTask(taskResumEventArg.Session);//继续任务

            TaskScheduleTrigger.RemoveScheduleTask(this);
        }
    }

    public class ApplicationCreatingTimeOutSuspendTaskContext : TaskScheduleContextBase
    {
        /// <summary>
        /// 间隔2秒未创建完成则判定创建超时
        /// </summary>
        public override TimeSpan Interval { get; set; } = new TimeSpan(0, 0, 2);

        /// <summary>
        /// 应用
        /// </summary>
        public IApplication Application { get; set; }

        /// <summary>
        /// 超时移除
        /// </summary>
        public override void Execute()
        {
            //释放资源
            var adapterPropertys = Application.GetApplicationAdapterProperty();
            foreach (var property in adapterPropertys)
            {
                var adapter = property.GetValue(Application) as ApplicationAdapterHandler;
                if (!adapter.IsNull() && !adapter.IsManualClose() && adapter.GetAttachedConnectionState())
                    adapter.CloseSession();
            }
            TaskScheduleTrigger.RemoveScheduleTask(this);
        }
    }
}
