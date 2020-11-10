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
        /// 主题
        /// </summary>
        string Topic { get; set; }

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
        public string Topic { get; set; }

        public virtual TimeSpan Interval { get; set; }

        public DateTime TimePoint { get; set; } = DateTime.Now;

        public abstract void Execute();
    }

    /// <summary>
    /// 离线处理器上下文，等待重连
    /// </summary>
    public class SuspendTaskContext : TaskScheduleContextBase, ICustomEvent
    {
        /// <summary>
        /// 间隔5秒执行一次
        /// </summary>
        public override TimeSpan Interval => new TimeSpan(0, 0, 5);

        public IList<SessionSyncContext> SessionSyncContexts { get; set; }

        public ApplicationAdapterHandler ApplicationAdapterHandler { get; set; }

        public override void Execute()
        {
            string id = ApplicationAdapterHandler.IdentifyId.Split('|')[0];
            var syncContext = SessionSyncContexts.FirstOrDefault(x => x[SysConstants.IdentifyId].ConvertTo<string>() == id);
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
            tokens[SysConstants.INDEX_WORKTYPE] = SessionKind.APP_SERVICE;
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
        /// 间隔5秒未创建完成则判定创建超时
        /// </summary>
        public override TimeSpan Interval => new TimeSpan(0, 0, 5);

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
