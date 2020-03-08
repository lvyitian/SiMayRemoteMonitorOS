using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UserControls
{
    public class ProcListViewItem : ListViewItem
    {
        public ProcListViewItem(int id, string processName, string windowName, int windowHandler, int memorySize, int threadCount, string filePath, int sessionId, string user)
            => this.Update(id, processName, windowName, windowHandler, memorySize, threadCount, filePath, sessionId, user);

        public void Update(int id, string processName, string windowName, int windowHandler, int memorySize, int threadCount, string filePath, int sessionId, string user)
        {
            this.SubItems.Clear();

            this.ProcessId = id;
            this.ProcessName = processName;
            this.WindowName = windowName;
            this.WindowHandler = windowHandler;
            this.ProcessMemorySize = memorySize;
            this.ProcessThreadCount = threadCount;
            this.SessionId = sessionId;
            this.User = user;

            this.Text = processName;
            this.SubItems.Add(windowName);
            this.SubItems.Add(windowHandler.ToString());
            this.SubItems.Add(((double)memorySize / 1024).ToString("0.0000") + " MB");
            this.SubItems.Add(threadCount.ToString());
            this.SubItems.Add(sessionId.ToString());
            this.SubItems.Add(user);
            this.SubItems.Add(filePath);
        }

        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public string WindowName { get; set; }

        public int WindowHandler { get; set; }

        public int ProcessMemorySize { get; set; }

        public int ProcessThreadCount { get; set; }

        public int SessionId { get; set; }
        public string User { get; set; }
    }

    public class SessionViewItem : ListViewItem
    {
        public SessionViewItem(string userName, int sessionId, int state, string windowStationName, bool hasUserProcess)
        {
            this.UserName = userName;
            this.SessionId = sessionId;
            this.SessionState = state;
            this.WindowStationName = windowStationName;
            this.HasUserProcess = hasUserProcess;

            this.Text = userName;
            this.SubItems.Add(sessionId.ToString());
            this.SubItems.Add(GetStateDescribe(state));
            this.SubItems.Add(windowStationName);
            this.SubItems.Add(hasUserProcess ? "是" : "否");
        }

        public string UserName { get; set; }
        public int SessionId { get; set; }
        public int SessionState { get; set; }
        public string WindowStationName { get; set; }
        public bool HasUserProcess { get; set; }

        private string GetStateDescribe(int state)
        {
            string describe = string.Empty;
            switch (state)
            {
                case 0:
                    describe = "活动的";
                    break;
                case 1:
                    describe = "已连接到客户端";
                    break;
                case 2:
                    describe = "正在连接到客户端";
                    break;
                case 3:
                    describe = "正在遮盖另一个WinStation";
                    break;
                case 4:
                    describe = "处于活动状态，但客户端已断开连接";
                    break;
                case 5:
                    describe = "正在等待客户端连接";
                    break;
                case 6:
                    describe = "正在监听连接";
                    break;
                case 7:
                    describe = "正在重置";
                    break;
                case 8:
                    describe = "因错误而关闭";
                    break;
                case 9:
                    describe = "正在初始化";
                    break;
            }

            return describe;
        }
    }
}
