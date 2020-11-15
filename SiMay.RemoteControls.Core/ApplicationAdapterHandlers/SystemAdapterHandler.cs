using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControls.Core
{
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_SYSMANAGER)]
    public class SystemAdapterHandler : ApplicationBaseAdapterHandler
    {
        public async Task<SystemInfoItem[]> GetSystemInfoItems()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_SYSTEMINFO);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = responsed.Datas.GetMessageEntity<ProcessPacket>();
                return pack.SystemInfos;
            }

            return null;
        }


        public async Task<ProcessItem[]> GetProcessList()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_PROCESS_LIST);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = responsed.Datas.GetMessageEntity<ProcessListPack>();
                return pack.ProcessList;
            }

            return null;
        }

        public async Task<(string cpuusage, string memoryUsage)> GetOccupy()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_GET_OCCUPY);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var pack = responsed.Datas.GetMessageEntity<SystemOccupyPack>();
                return (pack.CpuUsage, pack.MemoryUsage);
            }

            return (null, null);
        }

        public async Task SetProcessWindowMaxi(IEnumerable<int> pids)
        {
            await SetProcessWindowState(1, pids);
        }

        public async Task SetProcessWindowMize(IEnumerable<int> pids)
        {
            await SetProcessWindowState(0, pids);
        }

        public async Task<SessionItem[]> EnumSession()
        {
            var responsed = await SendTo(MessageHead.S_SYSTEM_ENUMSESSIONS);
            if (!responsed.IsNull() && responsed.IsOK && !responsed.Datas.IsNullOrEmpty())
            {
                var sessionLst = responsed.Datas.GetMessageEntity<SessionsPacket>();
                return sessionLst.Sessions;
            }

            return null;
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
                 new KillPacket()
                 {
                     ProcessIds = pids.ToArray()
                 });
        }

        private async Task SetProcessWindowState(int state, IEnumerable<int> pids)
        {
            await SendTo(MessageHead.S_SYSTEM_MAXIMIZE,
                new SetWindowStatusPacket()
                {
                    State = state,
                    Handlers = pids.ToArray()
                });
        }

    }
}
