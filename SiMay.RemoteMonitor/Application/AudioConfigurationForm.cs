using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    public partial class AudioConfigurationForm : Form
    {
        public AudioConfigurationForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AppConfiguration.AudioSamplesPerSecond = int.Parse(nSamplesPerSec.Text);
            AppConfiguration.AudioBitsPerSample = int.Parse(wBitsPerSample.Text);
            AppConfiguration.AudioChannels = int.Parse(nChannels.Text);
            MessageBox.Show("设置保存成功,但设置需重新打开语音功能模块后才能生效。", "提示", 0, MessageBoxIcon.Exclamation);
            this.Close();
        }

        private void AudioConfigurationManager_Load(object sender, EventArgs e)
        {
            nSamplesPerSec.Text = AppConfiguration.AudioSamplesPerSecond.ToString();
            wBitsPerSample.Text = AppConfiguration.AudioBitsPerSample.ToString();
            nChannels.Text = AppConfiguration.AudioChannels.ToString();
        }
    }
}
