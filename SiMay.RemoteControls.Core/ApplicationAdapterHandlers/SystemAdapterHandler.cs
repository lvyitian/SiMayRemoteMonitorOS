using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    [ApplicationKeyAttribute(ApplicationKeyConstant.REMOTE_SYSMANAGER)]
    public class SystemAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<SystemAdapterHandler, IEnumerable<ProcessItem>> OnProcessListHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SystemInfoItem>> OnSystemInfoHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SessionItem>> OnSessionsEventHandler;

        public event Action<SystemAdapterHandler, string, string> OnOccupyHandlerEvent;

        public async Task GetSystemInfoItems()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_SYSTEMINFO);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = SiMay.Serialize.Standard.PacketSerializeHelper.DeserializePacket<SystemInfoPacket>(responsed.Datas);
                OnSystemInfoHandlerEvent?.Invoke(this, pack.SystemInfos);
            }
        }

        public async Task GetProcessList()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_PROCESS_LIST);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = SiMay.Serialize.Standard.PacketSerializeHelper.DeserializePacket<ProcessListPack>(responsed.Datas);
                OnProcessListHandlerEvent?.Invoke(this, pack.ProcessList);
            }
        }

        public async Task GetOccupy()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_OCCUPY);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = SiMay.Serialize.Standard.PacketSerializeHelper.DeserializePacket<SystemOccupyPack>(responsed.Datas);
                OnOccupyHandlerEvent?.Invoke(this, pack.CpuUsage, pack.MemoryUsage);
            }
        }

        public async Task SetProcessWindowMaxi(IEnumerable<int> pids)
        {
            await SetProcessWindowState(1, pids);
        }

        public async Task SetProcessWindowMize(IEnumerable<int> pids)
        {
            await SetProcessWindowState(0, pids);
        }

        public async Task EnumSession()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_ENUMSESSIONS);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var sessionLst = SiMay.Serialize.Standard.PacketSerializeHelper.DeserializePacket<SessionsPacket>(responsed.Datas);
                this.OnSessionsEventHandler?.Invoke(this, sessionLst.Sessions);
            }
        }

        public async Task CreateProcessAsUser(int sessionId)
        {
            await SendTo(MessageHead.S_SYSTEM_CREATE_USER_PROCESS,
                new CreateProcessAsUserPack()
                {
                    SessionId = sessionId
                });
        }

        public async Task KillProcess(IEnumerable<int> pids)
        {
            await SendTo(MessageHead.S_SYSTEM_KILL,
                 new KillProcessPacket()
                 {
                     ProcessIds = pids.ToArray()
                 });
            await GetProcessList();
        }

        private async Task SetProcessWindowState(int state, IEnumerable<int> pids)
        {
            await SendTo(MessageHead.S_SYSTEM_MAXIMIZE,
                new SysWindowMaxPacket()
                {
                    State = state,
                    Handlers = pids.ToArray()
                });
        }

    }
}
