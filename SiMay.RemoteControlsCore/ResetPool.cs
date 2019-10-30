using SiMay.Basic;
using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SiMay.RemoteControlsCore
{
    public class ResetPool
    {
        private bool _isRun = true;
        private List<SessionSyncContext> _syncContexts;
        private List<SuspendTaskContext> _suspendWorkQueue = new List<SuspendTaskContext>();
        public ResetPool(List<SessionSyncContext> syncContexts)
        {
            _syncContexts = syncContexts;
            ThreadHelper.CreateThread(workerMethod, true);
        }


        private void workerMethod()
        {
            while (_isRun)
            {
                for (int i = 0; i < _suspendWorkQueue.Count; i++)
                {
                    SuspendTaskContext task = _suspendWorkQueue[i];

                    //if ((int)(DateTime.Now - task.DisconnectTime).TotalSeconds > 60 * 5)
                    //{
                    //    LogHelper.WriteErrorByCurrentMethod("Reset TimeOut--{0},{1}".FormatTo(task.Adapter.ResetMsg, task.Adapter.IdentifyId));
                    //    //如果超时5分钟未重连就判定该任务已真正离线
                    //    task.Adapter.WindowClose();//关闭窗口
                    //    _suspendWorkQueue.Remove(task);
                    //    i--;
                    //}
                    //else
                    //{
                    string id = task.AdapterHandler.IdentifyId.Split('|')[0];
                    var syncContext = _syncContexts.FirstOrDefault(x => x.KeyDictions[SysConstants.IdentifyId].ConvertTo<string>() == id);

                    LogHelper.WriteErrorByCurrentMethod("beigin Reset--{0},{1},{2}".FormatTo(task.AdapterHandler.ResetApplicationKey, task.AdapterHandler.IdentifyId, id));

                    if (syncContext != null)
                    {
                        if (task.AdapterHandler.IsClose)
                        {
                            //窗口关闭将不再建立连接
                            _suspendWorkQueue.Remove(task);
                            i--;
                            continue;
                        }
                        byte[] data = MessageHelper.CopyMessageHeadTo(MessageHead.S_MAIN_ACTIVATE_CTRLSERVICE,
                            task.AdapterHandler.ResetApplicationKey);
                        syncContext.Session.SendMessageDoHasCOM(data);

                        LogHelper.WriteErrorByCurrentMethod("send reset command--{0},{1},{2}".FormatTo(task.AdapterHandler.ResetApplicationKey, task.AdapterHandler.IdentifyId, id));
                    }

                    //}
                }
                Thread.Sleep(20000);
            }
        }
        public void Put(SuspendTaskContext context)
        {
            _suspendWorkQueue.Add(context);
            LogHelper.WriteErrorByCurrentMethod("Session Close--{0},{1}".FormatTo(context.AdapterHandler.ResetApplicationKey, context.AdapterHandler.IdentifyId));
        }
        public SuspendTaskContext FindTask(string identifyId)
        {
            var task = _suspendWorkQueue
                .Where(x => x.AdapterHandler.IdentifyId.Split('|').FirstOrDefault() == identifyId)
                .FirstOrDefault();

            return task;
        }

        public bool RemoveTask(string identifyId)
        {
            var task = _suspendWorkQueue.Where(x => x.AdapterHandler.IdentifyId.Split('|').FirstOrDefault() == identifyId).FirstOrDefault();
            if (task != null)
            {
                _suspendWorkQueue.Remove(task);
                LogHelper.WriteErrorByCurrentMethod("ResetTask Remove--{0},{1}".FormatTo(task.AdapterHandler.ResetApplicationKey, task.AdapterHandler.IdentifyId));
            }
            else
                return false;

            return true;
        }
        public void Stop()
        {
            this._isRun = false;
        }
    }
}
