using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using WindSound;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(20, "语音监听", "RemoteAudioJob", "AudioManager")]
    public partial class AudioManager : Form, IControlSource
    {
        private string _title = "//远程语音监听 #Name#";
        private bool _isRecord = false;
        private DateTime _runtimeSpan;
        private FileStream _fileStream;
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public AudioManager(MessageAdapter adapter)
        {
            _adapter = adapter;

            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            adapter.ResetMsg = this.GetType().GetAppKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();

        private void Adapter_OnSessionNotifyPro(SessionHandler session, SessionNotifyType notify)
        {
            switch (notify)
            {
                case SessionNotifyType.Message:
                    if (this == null || this.IsDisposed || _isRun == false)
                        return;

                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case SessionNotifyType.OnReceive:
                    break;
                case SessionNotifyType.ContinueTask:
                    this.Text = _title;
                    break;
                case SessionNotifyType.SessionClosed:
                    this.Text = _title + " [" + _adapter.TipText + "]";
                    break;
                case SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case SessionNotifyType.WindowClose:
                    _adapter.WindowClosed = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        private int recvLen = 0;
        private int sendLen = 0;
        private bool _isRun = false;
        private bool IsPlaying = true;
        private int SoundBufferCount = 8;
        private WinSoundRecord _recorder;
        private WinSoundPlayer _player;
        private void AudioManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;

            this._runtimeSpan = DateTime.Now;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += (s, ex) =>
            {
                try
                {
                    this.Invoke(new Action(() => this.labRuntime.Text = "已运行:" + (DateTime.Now - this._runtimeSpan).ToString("dd\\.hh\\.mm\\.ss")));
                }
                catch
                {
                    timer.Close();
                    timer.Dispose();
                }
            };
            timer.Start();

            Initialize();
        }

        private void Initialize()
        {
            int samplesPerSecond = int.Parse(AppConfiguration.AudioSamplesPerSecond);
            int bitsPerSample = int.Parse(AppConfiguration.AudioBitsPerSample);
            int channels = int.Parse(AppConfiguration.AudioChannels);

            string waveOutDeviceName = WinSound.GetWaveOutDeviceNames().Count > 0 ? WinSound.GetWaveOutDeviceNames()[0] : null;

            if (waveOutDeviceName != null)
            {
                _player = new WinSoundPlayer();
                _player.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, SoundBufferCount);
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("本机未找到播放设备!");
            }

            string waveInDeviceName = WinSound.GetWaveInDeviceNames().Count > 0 ? WinSound.GetWaveInDeviceNames()[0] : null;

            if (waveInDeviceName != null)
            {
                _recorder = new WinSoundRecord();
                _recorder.DataRecorded += Recorder_DataRecorded;
                _recorder.Open(waveInDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, SoundBufferCount);
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("本机未找到录音设备!");
            }

            OpenRemoteAudio(samplesPerSecond, bitsPerSample, channels);

            _isRun = true;
        }

        private void OpenRemoteAudio(int samplesPerSecond, int bitsPerSample, int channels)
        {
            _adapter.SendAsyncMessage(MessageHead.S_AUDIO_START, new AudioOptionsPack()
            {
                SamplesPerSecond = samplesPerSecond,
                BitsPerSample = bitsPerSample,
                Channels = channels
            });
        }

        private void Recorder_DataRecorded(byte[] bytes)
        {
            if (IsPlaying) return; //正在播放远程语音，不发送数据

            if (bytes == null) return;

            _adapter.SendAsyncMessage(MessageHead.S_AUDIO_DATA, bytes);

            this.Invoke(new Action(() =>
            {
                sendLen += bytes.Length;
                sendataLen.Text = (sendLen / 1024).ToString() + " KB";
            }));
        }

        public void SetWindowText(string value)
        {
            this.Text = value;
        }
        public void ContinueTask()
        {

        }
        public void OnReceive(SessionHandler session)
        {
        }

        [PacketHandler(MessageHead.C_AUDIO_DEVICE_OPENSTATE)]
        public void RemoteDeveiceStatusHandler(SessionHandler session)
        {
            var statesPack = session.CompletedBuffer.GetMessageEntity<AudioDeviceStatesPack>();
            if (!statesPack.PlayerEnable && !statesPack.RecordEnable)
                tip.Text = "远程计算机的播放设备与录音设备未找到!";
            else if (statesPack.PlayerEnable && !statesPack.RecordEnable)
                tip.Text = "远程录音设备未找到,但可向远程发起说话...........";
            else if (!statesPack.PlayerEnable && statesPack.RecordEnable)
                tip.Text = "正在监听远程声音,但远程播放设备未找到...........";
            else if (statesPack.PlayerEnable && statesPack.RecordEnable)
                tip.Text = "正在监听远程声音,并且您可以向远程发起说话...........";
        }

        [PacketHandler(MessageHead.C_AUDIO_DATA)]
        public async void PlayerData(SessionHandler session)
        {
            var payload = session.CompletedBuffer.GetMessagePayload();
            if (_isRun && _player != null && IsPlaying == true)
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        recvLen += payload.Length;
                        recvdataLen.Text = (recvLen / 1024).ToString() + " KB";
                    }));
                    _player.PlayData(payload);
                    if (this._isRecord)
                        await _fileStream.WriteAsync(payload, 0, payload.Length);
                }
                catch { }
            }
        }

        private void AudioManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_recorder != null)
                _recorder.Stop();

            if (_player != null)
                _player.Close();

            _isRun = false;
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (MessageBox.Show("确认向远程发送本地声音吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                {
                    checkBox1.Checked = false;
                    return;
                }
                IsPlaying = false;
                _adapter.SendAsyncMessage(MessageHead.S_AUDIO_DEIVCE_ONOFF, "0");
            }
            else
            {
                _adapter.SendAsyncMessage(MessageHead.S_AUDIO_DEIVCE_ONOFF, "1");
                IsPlaying = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            AudioConfigurationForm dlg = new AudioConfigurationForm();
            dlg.ShowDialog();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this._isRecord = checkBox2.Checked;
            if (checkBox2.Checked)
                _fileStream = new FileStream(DateTime.Now.ToFileTime() + ".PCM", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            else
            {
                string name = _fileStream.Name;
                _fileStream.Flush();
                _fileStream.Close();
                MessageBoxHelper.ShowBoxExclamation("录音已完成,文件位于:" + name);
            }
        }
    }
}