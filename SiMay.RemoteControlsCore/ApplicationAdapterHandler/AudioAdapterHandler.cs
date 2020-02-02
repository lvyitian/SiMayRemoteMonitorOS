using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class AudioAdapterHandler : ApplicationAdapterHandler
    {
        public event Action<AudioAdapterHandler, bool, bool> OnOpenDeviceStatusEventHandler;

        public event Action<AudioAdapterHandler, byte[]> OnPlayerEventHandler;

        [PacketHandler(MessageHead.C_AUDIO_DEVICE_OPENSTATE)]
        private void RemoteDeveiceStatusHandler(SessionProviderContext session)
        {
            var statesPack = GetMessageEntity<AudioDeviceStatesPack>(session);
            this.OnOpenDeviceStatusEventHandler?.Invoke(this, statesPack.PlayerEnable, statesPack.RecordEnable);
        }

        [PacketHandler(MessageHead.C_AUDIO_DATA)]
        private void PlayerData(SessionProviderContext session)
        {
            var payload = GetMessage(session);
            this.OnPlayerEventHandler?.Invoke(this, payload);
        }

        public void StartRemoteAudio(int samplesPerSecond, int bitsPerSample, int channels)
        {
            SendTo(CurrentSession, MessageHead.S_AUDIO_START,
                new AudioOptionsPack()
                {
                    SamplesPerSecond = samplesPerSecond,
                    BitsPerSample = bitsPerSample,
                    Channels = channels
                });
        }

        /// <summary>
        /// 发送声音到远程
        /// </summary>
        /// <param name="payload"></param>
        public void SendVoiceDataToRemote(byte[] payload)
        {
            SendTo(CurrentSession, MessageHead.S_AUDIO_DATA, payload);
        }


        /// <summary>
        /// 设置远程启用发送语音流
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRemotePlayerStreamEnabled(bool enabled)
        {
            SendTo(CurrentSession, MessageHead.S_AUDIO_DEIVCE_ONOFF, enabled ? "1" : "0");
        }
    }
}
