using Microsoft.Win32;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Enums;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.Startup;
using SiMay.Core.Packets.Startup.Enums;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.ServiceCore.ServiceSource;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.ServiceCore.ControlService
{
    [ServiceName("启动项管理")]
    [ServiceKey("StartupManagerJob")]
    public class StartupService : ServiceManager, IServiceSource
    {
        private PacketModelBinder<TcpSocketSaeaSession> _handlerBinder = new PacketModelBinder<TcpSocketSaeaSession>();
        public override void OnNotifyProc(TcpSocketCompletionNotify notify, TcpSocketSaeaSession session)
        {
            switch (notify)
            {
                case TcpSocketCompletionNotify.OnConnected:
                    break;
                case TcpSocketCompletionNotify.OnSend:
                    break;
                case TcpSocketCompletionNotify.OnDataReceiveing:
                    break;
                case TcpSocketCompletionNotify.OnDataReceived:
                    this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case TcpSocketCompletionNotify.OnClosed:
                    this._handlerBinder.Dispose();
                    break;
            }
        }

        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public void InitializeComplete(TcpSocketSaeaSession session)
        {
            SendAsyncToServer(MessageHead.C_MAIN_ACTIVE_APP,
                new ActiveAppPack()
                {
                    IdentifyId = AppConfiguartion.IdentifyId,
                    ServiceKey = this.GetType().GetServiceKey(),
                    OriginName = Environment.MachineName + "@" + (AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo)
                });
        }
        [PacketHandler(MessageHead.S_GLOBAL_ONCLOSE)]
        public void CloseSession(TcpSocketSaeaSession session)
            => this.CloseSession();

        [PacketHandler(MessageHead.S_STARTUP_GET_LIST)]
        public void HandleGetStartupItems(TcpSocketSaeaSession session)
        {
            try
            {
                List<StartupItemPack> startupItems = new List<StartupItemPack>();

                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItemPack
                            { Name = item.Item1, Path = item.Item2, Type = StartupType.LocalMachineRun });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItemPack
                            { Name = item.Item1, Path = item.Item2, Type = StartupType.LocalMachineRunOnce });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItemPack
                            { Name = item.Item1, Path = item.Item2, Type = StartupType.CurrentUserRun });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItemPack
                            { Name = item.Item1, Path = item.Item2, Type = StartupType.CurrentUserRunOnce });
                        }
                    }
                }
                if (PlatformHelper.Is64Bit)
                {
                    using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"))
                    {
                        if (key != null)
                        {
                            foreach (var item in key.GetKeyValues())
                            {
                                startupItems.Add(new StartupItemPack
                                { Name = item.Item1, Path = item.Item2, Type = StartupType.LocalMachineWoW64Run });
                            }
                        }
                    }
                    using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                    {
                        if (key != null)
                        {
                            foreach (var item in key.GetKeyValues())
                            {
                                startupItems.Add(new StartupItemPack
                                {
                                    Name = item.Item1,
                                    Path = item.Item2,
                                    Type = StartupType.LocalMachineWoW64RunOnce
                                });
                            }
                        }
                    }
                }
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup)))
                {
                    var files = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup)).GetFiles();

                    startupItems.AddRange(files.Where(file => file.Name != "desktop.ini").Select(file => new StartupItemPack
                    { Name = file.Name, Path = file.FullName, Type = StartupType.StartMenu }));
                }

                SendAsyncToServer(MessageHead.C_STARTUP_LIST, new StartupItemsPack()
                {
                    StartupItems = startupItems.ToArray()
                });
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
                SendAsyncToServer(MessageHead.C_STARTUP_OPER_RESPONSE, new StartupOperResponsePack()
                {
                    OperFlag = OperFlag.GetStartupItems,
                    Successed = false,
                    Msg = ex.Message
                });
            }
        }
        [PacketHandler(MessageHead.S_STARTUP_ADD_ITEM)]
        public void HandleDoStartupItemAdd(TcpSocketSaeaSession session)
        {
            try
            {
                var command = session.CompletedBuffer.GetMessageEntity<StartupItemPack>();
                switch (command.Type)
                {
                    case StartupType.LocalMachineRun:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.LocalMachineRunOnce:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.CurrentUserRun:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.CurrentUserRunOnce:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.LocalMachineWoW64Run:
                        if (!PlatformHelper.Is64Bit)
                            throw new NotSupportedException("Only on 64-bit systems supported");

                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.LocalMachineWoW64RunOnce:
                        if (!PlatformHelper.Is64Bit)
                            throw new NotSupportedException("Only on 64-bit systems supported");

                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name, command.Path, true))
                        {
                            throw new Exception("Could not add value");
                        }
                        break;
                    case StartupType.StartMenu:
                        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup)))
                        {
                            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                        }

                        string lnkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                            command.Name + ".url");

                        using (var writer = new StreamWriter(lnkPath, false))
                        {
                            writer.WriteLine("[InternetShortcut]");
                            writer.WriteLine("URL=file:///" + command.Path);
                            writer.WriteLine("IconIndex=0");
                            writer.WriteLine("IconFile=" + command.Path.Replace('\\', '/'));
                            writer.Flush();
                        }
                        break;
                }
                this.HandleGetStartupItems(session);
            }
            catch (Exception ex)
            {
                SendAsyncToServer(MessageHead.C_STARTUP_OPER_RESPONSE, new StartupOperResponsePack()
                {
                    OperFlag = OperFlag.AddStartupItem,
                    Successed = false,
                    Msg = ex.Message
                });
            }
        }

        [PacketHandler(MessageHead.S_STARTUP_REMOVE_ITEM)]
        public void HandleDoStartupItemRemove(TcpSocketSaeaSession session)
        {
            try
            {
                var pack = session.CompletedBuffer.GetMessageEntity<StartupItemsPack>();
                foreach (var command in pack.StartupItems)
                {
                    switch (command.Type)
                    {
                        case StartupType.LocalMachineRun:
                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.LocalMachineRunOnce:
                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.CurrentUserRun:
                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.CurrentUserRunOnce:
                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.LocalMachineWoW64Run:
                            if (!PlatformHelper.Is64Bit)
                                throw new NotSupportedException("Only on 64-bit systems supported");

                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                                "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.LocalMachineWoW64RunOnce:
                            if (!PlatformHelper.Is64Bit)
                                throw new NotSupportedException("Only on 64-bit systems supported");

                            if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                                "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce", command.Name))
                            {
                                throw new Exception("Could not remove value");
                            }
                            break;
                        case StartupType.StartMenu:
                            string startupItemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), command.Name);

                            if (!File.Exists(startupItemPath))
                                throw new IOException("File does not exist");

                            File.Delete(startupItemPath);
                            break;
                    }
                }
                this.HandleGetStartupItems(session);
            }
            catch (Exception ex)
            {
                SendAsyncToServer(MessageHead.C_STARTUP_OPER_RESPONSE, new StartupOperResponsePack()
                {
                    OperFlag = OperFlag.RemoveStartupItem,
                    Successed = false,
                    Msg = ex.Message
                });
            }
        }
    }
}
