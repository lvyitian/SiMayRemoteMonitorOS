using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SiMay.Basic;
using SiMay.RemoteControls.Core;

namespace SiMay.RemoteMonitor.UserControls
{
    public class USessionListItem : ListViewItem
    {
        public SessionSyncContext SessionSyncContext { get; set; }
        public USessionListItem(SessionSyncContext syncContext)
        {
            SessionSyncContext = syncContext;
            UpdateListItemText();
        }

        public void UpdateListItemText()
        {
            this.SubItems.Clear();
            this.Text = SessionSyncContext[SysConstants.IPV4].ConvertTo<string>();
            this.SubItems.Add(SessionSyncContext[SysConstants.MachineName].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext[SysConstants.OSVersion].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext[SysConstants.ProcessorInfo].ConvertTo<string>());
            this.SubItems.Add("1*" + SessionSyncContext[SysConstants.ProcessorCount].ConvertTo<int>());
            this.SubItems.Add(SessionSyncContext[SysConstants.MemorySize].ConvertTo<long>() / 1024 / 1024 + "MB");
            this.SubItems.Add(SessionSyncContext[SysConstants.UserName].ConvertTo<string>());

            //if (SessionSyncContext[SysConstants.HasLoadServiceCOM].ConvertTo<bool>())
            //{
            this.SubItems.Add(SessionSyncContext[SysConstants.ExistCameraDevice].ConvertTo<bool>() ? "YES" : "NO");
            this.SubItems.Add(SessionSyncContext[SysConstants.ExitsRecordDevice].ConvertTo<bool>() ? "YES" : "NO");
            this.SubItems.Add(SessionSyncContext[SysConstants.ExitsPlayerDevice].ConvertTo<bool>() ? "YES" : "NO");
            //}
            //else
            //{
            //    //插件未加载
            //    this.SubItems.Add("Unknown");
            //    this.SubItems.Add("Unknown");
            //    this.SubItems.Add("Unknown");
            //}
            this.SubItems.Add(SessionSyncContext[SysConstants.Remark].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext[SysConstants.ServiceVison].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext[SysConstants.StartRunTime].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext[SysConstants.GroupName].ConvertTo<string>());
        }
    }
}
