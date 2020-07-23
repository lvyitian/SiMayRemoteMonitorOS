using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.ServiceCore.Attributes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CommonWin32Api = SiMay.Platform.Windows.CommonWin32Api;

namespace SiMay.ServiceCore
{
    [ServiceName("Tcp连接管理")]
    [ApplicationKeyAttribute(ApplicationKeyConstant.REMOTE_TCP)]
    public class TcpConnectionService : ApplicationRemoteService
    {
        public override void SessionInited(SessionProviderContext session)
        {

        }

        public override void SessionClosed()
        {

        }

        [PacketHandler(MessageHead.S_TCP_GET_LIST)]
        public void GetTcpConnectionList(SessionProviderContext session)
        {
            var table = GetTable();

            var connections = new TcpConnectionItem[table.Length];

            for (int i = 0; i < table.Length; i++)
            {
                string processName;
                try
                {
                    var p = Process.GetProcessById((int)table[i].owningPid);
                    processName = p.ProcessName;
                }
                catch
                {
                    processName = $"PID: {table[i].owningPid}";
                }
                var s = table[i];
                connections[i] = new TcpConnectionItem
                {
                    ProcessName = processName,
                    LocalAddress = table[i].LocalAddress.ToString(),
                    LocalPort = table[i].LocalPort.ToString(),
                    RemoteAddress = table[i].RemoteAddress.ToString(),
                    RemotePort = table[i].RemotePort.ToString(),
                    State = (TcpConnectionState)table[i].state
                };
                var ss = connections[i];
            }

            CurrentSession.SendTo(MessageHead.C_TCP_LIST,
                new TcpConnectionPacket()
                {
                    TcpConnections = connections
                });
        }

        [PacketHandler(MessageHead.S_TCP_CLOSE_CHANNEL)]
        public void CloseTcpConnectionHandler(SessionProviderContext session)
        {
            var kills = session.GetMessageEntity<KillTcpConnectionPack>();

            var table = GetTable();

            foreach (var packet in kills.Kills)
            {
                var s = table.Where(c => c.RemotePort == 5200).ToList();
                for (var i = 0; i < table.Length; i++)
                {
                    //search for connection
                    if (packet.LocalAddress == table[i].LocalAddress.ToString() &&
                        packet.LocalPort == table[i].LocalPort.ToString() &&
                        packet.RemoteAddress == table[i].RemoteAddress.ToString() &&
                        packet.RemotePort == table[i].RemotePort.ToString())
                    {
                        // it will close the connection only if client run as admin
                        //table[i].state = (byte)ConnectionStates.Delete_TCB;
                        table[i].state = 12; // 12 for Delete_TCB state
                        var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(table[i]));
                        Marshal.StructureToPtr(table[i], ptr, false);
                        var result = CommonWin32Api.SetTcpEntry(ptr);
                    }
                }
            }
            this.GetTcpConnectionList(session);
        }
        private static CommonWin32Api.MibTcprowOwnerPid[] GetTable()
        {
            CommonWin32Api.MibTcprowOwnerPid[] tTable;
            var afInet = 2;
            var buffSize = 0;
            var ret = CommonWin32Api.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, afInet, CommonWin32Api.TcpTableClass.TcpTableOwnerPidAll);
            var buffTable = Marshal.AllocHGlobal(buffSize);
            try
            {
                ret = CommonWin32Api.GetExtendedTcpTable(buffTable, ref buffSize, true, afInet, CommonWin32Api.TcpTableClass.TcpTableOwnerPidAll);
                if (ret != 0)
                    return null;
                var tab = (CommonWin32Api.MibTcptableOwnerPid)Marshal.PtrToStructure(buffTable, typeof(CommonWin32Api.MibTcptableOwnerPid));
                var rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                tTable = new CommonWin32Api.MibTcprowOwnerPid[tab.dwNumEntries];
                for (var i = 0; i < tab.dwNumEntries; i++)
                {
                    var tcpRow = (CommonWin32Api.MibTcprowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(CommonWin32Api.MibTcprowOwnerPid));
                    tTable[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }
            return tTable;
        }
    }
}
