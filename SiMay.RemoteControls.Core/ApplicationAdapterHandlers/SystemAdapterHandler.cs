using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class SystemAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<SystemAdapterHandler, IEnumerable<ProcessItem>> OnProcessListHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SystemInfoItem>> OnSystemInfoHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SessionItem>> OnSessionsEventHandler;

        public event Action<SystemAdapterHandler, string, string> OnOccupyHandlerEvent;

        [PacketHandler(MessageHead.C_SYSTEM_SYSTEMINFO)]
        private void HandlerProcessList(SessionProviderContext session)
        {
            var pack = session.GetMessageEntity<SystemInfoPack>();
            OnSystemInfoHandlerEvent?.Invoke(this, pack.SystemInfos);
        }

        [PacketHandler(MessageHead.C_SYSTEM_OCCUPY_INFO)]
        private void HandlerOccupy(SessionProviderContext session)
        {
            var pack = session.GetMessageEntity<SystemOccupyPack>();
            OnOccupyHandlerEvent?.Invoke(this, pack.CpuUsage, pack.MemoryUsage);
        }

        [PacketHandler(MessageHead.C_SYSTEM_PROCESS_LIST)]
        private void ProcessItemHandler(SessionProviderContext session)
        {
            var processLst = session.GetMessageEntity<ProcessListPack>().ProcessList;
            OnProcessListHandlerEvent?.Invoke(this, processLst);
        }
        [PacketHandler(MessageHead.C_SYSTEM_SESSIONS)]
        private void SessionsItemHandler(SessionProviderContext session)
        {
            var sessionLst = session.GetMessageEntity<SessionsPack>().Sessions;
            this.OnSessionsEventHandler?.Invoke(this, sessionLst);
        }

        public void GetSystemInfoItems()
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_GET_SYSTEMINFO);
        }

        public void GetProcessList()
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_GET_PROCESS_LIST);
        }

        public void GetOccupy()
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_GET_OCCUPY);
        }

        public void SetProcessWindowMaxi(IEnumerable<int> pids)
        {
            SetProcessWindowState(1, pids);
        }

        public void SetProcessWindowMize(IEnumerable<int> pids)
        {
            SetProcessWindowState(0, pids);
        }

        public void EnumSession()
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_ENUMSESSIONS);
        }

        public void CreateProcessAsUser(int sessionId)
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_CREATE_USER_PROCESS,
                new CreateProcessAsUserPack()
                {
                    SessionId = sessionId
                });
        }

        public void KillProcess(IEnumerable<int> pids)
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_KILL,
                new SysKillPack()
                {
                    ProcessIds = pids.ToArray()
                });
        }

        private void SetProcessWindowState(int state, IEnumerable<int> pids)
        {
            CurrentSession.SendTo(MessageHead.S_SYSTEM_MAXIMIZE,
                new SysWindowMaxPack()
                {
                    State = state,
                    Handlers = pids.ToArray()
                });
        }

    }
}
