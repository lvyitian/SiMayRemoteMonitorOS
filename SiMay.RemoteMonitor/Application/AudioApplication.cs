using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using WindSound;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("远程语音")]
    [AppResourceName("AudioManager")]
    [Application(typeof(AudioAdapterHandler), AppJobConstant.REMOTE_AUDIO, 20)]
    public partial class AudioApplication : Form, IApplication
    {
        [ApplicationAdapterHandler]
        public AudioAdapterHandler AudioAdapterHandler { get; set; }

        private string _title = "//远程语音监听 #Name#";

        private WinSoundRecord _recorder;
        private WinSoundPlayer _player;
        private FileStream _fileStream;
        private bool _isRun = false;
        private bool _isPlaying = true;
        private int _recvVoiceDataLength = 0;
        private int _sendVoiceDataLength = 0;
        private int _soundBufferCount = 8;

        private bool _isRecord = false;
        private DateTime _runtimeSpan;

        public AudioApplication()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Show();
        }

        public void SessionClose(ApplicationAdapterHandler handler)
        {
            this.Text = this._title + " [" + this.AudioAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            this.Text = this._title;
        }


        private void AudioManager_Load(object sender, EventArgs e)
        {
            this.Text = this._title = this._title.Replace("#Name#", this.AudioAdapterHandler.OriginName);
            this.AudioAdapterHandler.OnOpenDeviceStatusEventHandler += OnOpenDeviceStatusEventHandler;
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

        private async void OnPlayerEventHandler(AudioAdapterHandler adapterHandler, byte[] voiceData)
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
                        await this._fileStream.WriteAsync(voiceData, 0, voiceData.Length);
                }
                catch { }
            }
        }

        private void OnOpenDeviceStatusEventHandler(AudioAdapterHandler adapterHandler, bool playerState, bool recordState)
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

        private void Initialize()
        {
            int samplesPerSecond = AppConfiguration.AudioSamplesPerSecond;
            int bitsPerSample = AppConfiguration.AudioBitsPerSample;
            int channels = AppConfiguration.AudioChannels;

            string waveOutDeviceName = WinSound.GetWaveOutDeviceNames().Count > 0 ? WinSound.GetWaveOutDeviceNames()[0] : null;

            if (waveOutDeviceName != null)
            {
                _player = new WinSoundPlayer();
                _player.Open(waveOutDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, _soundBufferCount);
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
                _recorder.Open(waveInDeviceName, samplesPerSecond, bitsPerSample, channels, 1280, _soundBufferCount);
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("本机未找到录音设备!");
            }
            this.AudioAdapterHandler.StartRemoteAudio(samplesPerSecond, bitsPerSample, channels);

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
            this.AudioAdapterHandler.OnOpenDeviceStatusEventHandler -= OnOpenDeviceStatusEventHandler;
            this.AudioAdapterHandler.OnPlayerEventHandler -= OnPlayerEventHandler;
            this.AudioAdapterHandler.CloseSession();
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
                this._isPlaying = false;
                this.AudioAdapterHandler.SetRemotePlayerStreamEnabled(false);
            }
            else
            {
                this.AudioAdapterHandler.SetRemotePlayerStreamEnabled(true);
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