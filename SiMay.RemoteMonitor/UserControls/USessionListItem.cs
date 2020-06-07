using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SiMay.Basic;
using SiMay.RemoteControlsCore;

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
            this.Text = SessionSyncContext.KeyDictions[SysConstants.IPV4].ConvertTo<string>();
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.MachineName].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.OSVersion].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.ProcessorInfo].ConvertTo<string>());
            this.SubItems.Add("1*" + SessionSyncContext.KeyDictions[SysConstants.ProcessorCount].ConvertTo<int>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.MemorySize].ConvertTo<long>() / 1024 / 1024 + "MB");
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.UserName].ConvertTo<string>());

            //if (SessionSyncContext.KeyDictions[SysConstants.HasLoadServiceCOM].ConvertTo<bool>())
            //{
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.ExistCameraDevice].ConvertTo<bool>() ? "YES" : "NO");
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.ExitsRecordDevice].ConvertTo<bool>() ? "YES" : "NO");
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.ExitsPlayerDevice].ConvertTo<bool>() ? "YES" : "NO");
            //}
            //else
            //{
            //    //插件未加载
            //    this.SubItems.Add("Unknown");
            //    this.SubItems.Add("Unknown");
            //    this.SubItems.Add("Unknown");
            //}
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.Remark].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.ServiceVison].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.StartRunTime].ConvertTo<string>());
            this.SubItems.Add(SessionSyncContext.KeyDictions[SysConstants.GroupName].ConvertTo<string>());
        }
    }
}
