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
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class SystemAdapterHandler : AdapterHandlerBase
    {
        public event Action<SystemAdapterHandler, IEnumerable<ProcessItem>> OnProcessListHandlerEvent;

        public event Action<SystemAdapterHandler, IEnumerable<SystemInfoItem>> OnSystemInfoHandlerEvent;

        public event Action<SystemAdapterHandler, string, string> OnOccupyHandlerEvent;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        [PacketHandler(MessageHead.C_SYSTEM_SYSTEMINFO)]
        private void HandlerProcessList(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<SystemInfoPack>();
            OnSystemInfoHandlerEvent?.Invoke(this, pack.SystemInfos);
        }

        [PacketHandler(MessageHead.C_SYSTEM_OCCUPY_INFO)]
        private void HandlerOccupy(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<SystemOccupyPack>();
            OnOccupyHandlerEvent?.Invoke(this, pack.CpuUsage, pack.MemoryUsage);
        }

        [PacketHandler(MessageHead.C_SYSTEM_PROCESS_LIST)]
        private void ProcessItemHandler(SessionHandler session)
        {
            var processLst = session.CompletedBuffer.GetMessageEntity<ProcessListPack>().ProcessList;
            OnProcessListHandlerEvent?.Invoke(this, processLst);
        }

        public void GetSystemInfoItems()
        {
            SendAsyncMessage(MessageHead.S_SYSTEM_GET_SYSTEMINFO);
        }

        public void GetProcessList()
        {
            SendAsyncMessage(MessageHead.S_SYSTEM_GET_PROCESS_LIST);
        }

        public void GetOccupy()
        {
            SendAsyncMessage(MessageHead.S_SYSTEM_GET_OCCUPY);
        }

        public void SetProcessWindowMaxi(IEnumerable<int> pids)
        {
            SetProcessWindowState(1, pids);
        }

        public void SetProcessWindowMize(IEnumerable<int> pids)
        {
            SetProcessWindowState(0, pids);
        }

        public void KillProcess(IEnumerable<int> pids)
        {
            SendAsyncMessage(MessageHead.S_SYSTEM_KILL,
                new SysKillPack()
                {
                    ProcessIds = pids.ToArray()
                });
        }

        private void SetProcessWindowState(int state, IEnumerable<int> pids)
        {
            SendAsyncMessage(MessageHead.S_SYSTEM_MAXIMIZE,
                new SysWindowMaxPack()
                {
                    State = state,
                    Handlers = pids.ToArray()
                });
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
