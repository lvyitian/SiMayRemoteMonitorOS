using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.SysManager;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class SystemAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<SystemAdapterHandler, IEnumerable<ProcessItem>> OnProcessListHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SystemInfoItem>> OnSystemInfoHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SessionItem>> OnSessionsEventHandler;

        public event Action<SystemAdapterHandler, string, string> OnOccupyHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<UninstallInfo>> OnUninstallListEventHandler;


        [PacketHandler(MessageHead.C_SYSTEM_SYSTEMINFO)]
        private void HandlerProcessList(SessionProviderContext session)
        {
            var pack = GetMessageEntity<SystemInfoPack>(session);
            OnSystemInfoHandlerEvent?.Invoke(this, pack.SystemInfos);
        }

        [PacketHandler(MessageHead.C_SYSTEM_OCCUPY_INFO)]
        private void HandlerOccupy(SessionProviderContext session)
        {
            var pack = GetMessageEntity<SystemOccupyPack>(session);
            OnOccupyHandlerEvent?.Invoke(this, pack.CpuUsage, pack.MemoryUsage);
        }

        [PacketHandler(MessageHead.C_SYSTEM_PROCESS_LIST)]
        private void ProcessItemHandler(SessionProviderContext session)
        {
            var processLst = GetMessageEntity<ProcessListPack>(session).ProcessList;
            OnProcessListHandlerEvent?.Invoke(this, processLst);
        }
        [PacketHandler(MessageHead.C_SYSTEM_SESSIONS)]
        private void SessionsItemHandler(SessionProviderContext session)
        {
            var sessionLst = GetMessageEntity<SessionsPack>(session).Sessions;
            this.OnSessionsEventHandler?.Invoke(this, sessionLst);
        }

        public void GetSystemInfoItems()
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_GET_SYSTEMINFO);
        }

        public void GetProcessList()
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_GET_PROCESS_LIST);
        }

        public void GetOccupy()
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_GET_OCCUPY);
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
            SendTo(CurrentSession, MessageHead.S_SYSTEM_ENUMSESSIONS);
        }

        public void CreateProcessAsUser(int sessionId)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_CREATE_USER_PROCESS,
                new CreateProcessAsUserPack()
                {
                    SessionId = sessionId
                });
        }

        public void KillProcess(IEnumerable<int> pids)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_KILL,
                new SysKillPack()
                {
                    ProcessIds = pids.ToArray()
                });
        }

        private void SetProcessWindowState(int state, IEnumerable<int> pids)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_MAXIMIZE,
                new SysWindowMaxPack()
                {
                    State = state,
                    Handlers = pids.ToArray()
                });
        }

        public event Action<SystemAdapterHandler, IEnumerable<ServiceItem>> OnServicesListEventHandler;

        [PacketHandler(MessageHead.C_SYSTEM_SERVICE_LIST)]
        private void ServiceItemHandler(SessionProviderContext session)
        {
            var serviceList = GetMessageEntity<ServiceInfoPack>(session).ServiceList;
            OnServicesListEventHandler?.Invoke(this, serviceList);
        }

        public void Service_GetList()
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_SERVICE_LIST);
        }
        public void Service_Stop(ServiceItem serviceItems)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_SERVICE_STOP, serviceItems);
        }
        public void Service_Strat(ServiceItem serviceItems)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_SERVICE_START, serviceItems);
        }
        public void Service_ReStrat(ServiceItem serviceItems)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_SERVICE_RESTART, serviceItems);
        }
        public void Service_StartType_Set(ServiceItem serviceItems)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_SERVICE_STARTTYPE_SET, serviceItems);
        }

        [PacketHandler(MessageHead.C_SYSTEM_UNINSTALL_LIST)]
        private void UninstallInfoHandler(SessionProviderContext session)
        {
            var uninstallList = GetMessageEntity<UninstallInfoPack>(session).UninstallList;
            OnUninstallListEventHandler?.Invoke(this, uninstallList);
        }
        public void Uninstall_GetList()
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_UNINSTALL_LIST);
        }
        public void Uninstall_Un(UninstallInfo uninstallInfo)
        {
            SendTo(CurrentSession, MessageHead.S_SYSTEM_UNINSTALL_UN, uninstallInfo);
        }
    }
}
