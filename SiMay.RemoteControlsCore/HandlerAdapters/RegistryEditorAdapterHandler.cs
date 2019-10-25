using Microsoft.Win32;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.RegEdit;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class RegistryEditorAdapterHandler : AdapterHandlerBase
    {
        public delegate void KeysReceivedEventHandler(RegistryEditorAdapterHandler adapterHandler, string rootKey, RegSeekerMatch[] matches);
        public delegate void KeyCreatedEventHandler(RegistryEditorAdapterHandler adapterHandler, string parentPath, RegSeekerMatch match);
        public delegate void KeyDeletedEventHandler(RegistryEditorAdapterHandler adapterHandler, string parentPath, string subKey);
        public delegate void KeyRenamedEventHandler(RegistryEditorAdapterHandler adapterHandler, string parentPath, string oldSubKey, string newSubKey);
        public delegate void ValueCreatedEventHandler(RegistryEditorAdapterHandler adapterHandler, string keyPath, RegValueData value);
        public delegate void ValueDeletedEventHandler(RegistryEditorAdapterHandler adapterHandler, string keyPath, string valueName);
        public delegate void ValueRenamedEventHandler(RegistryEditorAdapterHandler adapterHandler, string keyPath, string oldValueName, string newValueName);
        public delegate void ValueChangedEventHandler(RegistryEditorAdapterHandler adapterHandler, string keyPath, RegValueData value);

        public event KeysReceivedEventHandler OnKeysReceivedEventHandler;
        public event KeyCreatedEventHandler OnKeyCreatedEventHandler;
        public event KeyDeletedEventHandler OnKeyDeletedEventHandler;
        public event KeyRenamedEventHandler OnKeyRenamedEventHandler;
        public event ValueCreatedEventHandler OnValueCreatedEventHandler;
        public event ValueDeletedEventHandler OnValueDeletedEventHandler;
        public event ValueRenamedEventHandler OnValueRenamedEventHandler;
        public event ValueChangedEventHandler OnValueChangedEventHandler;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        [PacketHandler(MessageHead.C_NREG_LOAD_REGKEYS)]
        private void AddKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRegistryKeysResponsePack>();
            var handler = OnKeysReceivedEventHandler;
            handler?.Invoke(this, pack.RootKey, pack.Matches);
        }

        [PacketHandler(MessageHead.C_NREG_CREATE_KEY_RESPONSE)]
        private void CreateNewKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetCreateRegistryKeyResponsePack>();
            var handler = OnKeyCreatedEventHandler;
            handler?.Invoke(this, pack.ParentPath, pack.Match);
        }

        [PacketHandler(MessageHead.C_NREG_DELETE_KEY_RESPONSE)]
        private void DeleteKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetDeleteRegistryKeyResponsePack>();
            var handler = OnKeyDeletedEventHandler;
            handler?.Invoke(this, pack.ParentPath, pack.KeyName);
        }

        [PacketHandler(MessageHead.C_NREG_RENAME_KEY_RESPONSE)]
        private void RenameKeyedHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRenameRegistryKeyResponsePack>();
            var handler = OnKeyRenamedEventHandler;
            handler?.Invoke(this, pack.ParentPath, pack.OldKeyName, pack.NewKeyName);
        }

        [PacketHandler(MessageHead.C_NREG_CREATE_VALUE_RESPONSE)]
        private void CreateValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetCreateRegistryValueResponsePack>();
            var handler = OnValueCreatedEventHandler;
            handler?.Invoke(this, pack.KeyPath, pack.Value);
        }


        [PacketHandler(MessageHead.C_NREG_DELETE_VALUE_RESPONSE)]
        private void DeleteValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetDeleteRegistryValueResponsePack>();
            var handler = OnValueDeletedEventHandler;
            handler?.Invoke(this, pack.KeyPath, pack.ValueName);
        }

        [PacketHandler(MessageHead.C_NREG_RENAME_VALUE_RESPONSE)]
        private void RenameValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetRenameRegistryValueResponsePack>();
            var handler = OnValueRenamedEventHandler;
            handler?.Invoke(this, pack.KeyPath, pack.OldValueName, pack.NewValueName);
        }


        [PacketHandler(MessageHead.C_NREG_CHANGE_VALUE_RESPONSE)]
        private void ChangeValueHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<GetChangeRegistryValueResponsePack>();
            var handler = OnValueChangedEventHandler;
            handler?.Invoke(this, pack.KeyPath, pack.Value);
        }

        /// <summary>
        /// Loads the registry keys of a given root key.
        /// </summary>
        /// <param name="rootKeyName">The root key name.</param>
        public void LoadRegistryKey(string rootKeyName)
        {
            SendAsyncMessage(MessageHead.S_NREG_LOAD_REGKEYS,
                new DoLoadRegistryKeyPack()
                {
                    RootKeyName = rootKeyName
                });
        }

        /// <summary>
        /// Creates a registry key at the given parent path.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        public void CreateRegistryKey(string parentPath)
        {
            SendAsyncMessage(MessageHead.S_NREG_CREATE_KEY,
                                new DoCreateRegistryKeyPack()
                                {
                                    ParentPath = parentPath
                                });
        }

        /// <summary>
        /// Deletes the given registry key.
        /// </summary>
        /// <param name="parentPath">The parent path of the registry key to delete.</param>
        /// <param name="keyName">The registry key name to delete.</param>
        public void DeleteRegistryKey(string parentPath, string keyName)
        {
            SendAsyncMessage(MessageHead.S_NREG_DELETE_KEY,
                                new DoDeleteRegistryKeyPack()
                                {
                                    ParentPath = parentPath,
                                    KeyName = keyName
                                });
        }

        /// <summary>
        /// Renames the given registry key.
        /// </summary>
        /// <param name="parentPath">The parent path of the registry key to rename.</param>
        /// <param name="oldKeyName">The old name of the registry key.</param>
        /// <param name="newKeyName">The new name of the registry key.</param>
        public void RenameRegistryKey(string parentPath, string oldKeyName, string newKeyName)
        {
            SendAsyncMessage(MessageHead.S_NREG_RENAME_KEY,
                                        new DoRenameRegistryKeyPack()
                                        {
                                            ParentPath = parentPath,
                                            OldKeyName = oldKeyName,
                                            NewKeyName = newKeyName
                                        });
        }

        /// <summary>
        /// Creates a registry key value.
        /// </summary>
        /// <param name="keyPath">The registry key path.</param>
        /// <param name="kind">The kind of registry key value.</param>
        public void CreateRegistryValue(string keyPath, RegistryValueKind kind)
        {
            SendAsyncMessage(MessageHead.S_NREG_CREATE_VALUE,
                                new DoCreateRegistryValuePack()
                                {
                                    KeyPath = keyPath,
                                    Kind = kind
                                });
        }

        /// <summary>
        /// Deletes the registry key value.
        /// </summary>
        /// <param name="keyPath">The registry key path.</param>
        /// <param name="valueName">The registry key value name to delete.</param>
        public void DeleteRegistryValue(string keyPath, string valueName)
        {
            SendAsyncMessage(MessageHead.S_NREG_DELETE_VALUE,
                                        new DoDeleteRegistryValuePack()
                                        {
                                            KeyPath = keyPath,
                                            ValueName = valueName
                                        });
        }

        /// <summary>
        /// Renames the registry key value.
        /// </summary>
        /// <param name="keyPath">The registry key path.</param>
        /// <param name="oldValueName">The old registry key value name.</param>
        /// <param name="newValueName">The new registry key value name.</param>
        public void RenameRegistryValue(string keyPath, string oldValueName, string newValueName)
        {
            SendAsyncMessage(MessageHead.S_NREG_RENAME_VALUE,
                                    new DoRenameRegistryValuePack()
                                    {
                                        KeyPath = keyPath,
                                        OldValueName = oldValueName,
                                        NewValueName = newValueName
                                    });
        }

        /// <summary>
        /// Changes the registry key value.
        /// </summary>
        /// <param name="keyPath">The registry key path.</param>
        /// <param name="value">The updated registry key value.</param>
        public void ChangeRegistryValue(string keyPath, RegValueData value)
        {
            SendAsyncMessage(MessageHead.S_NREG_CHANGE_VALUE,
                                    new DoChangeRegistryValuePack()
                                    {
                                        KeyPath = keyPath,
                                        Value = value
                                    });
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
