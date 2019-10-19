using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainForm
{
    partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LnkGithubPage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            this.lblVersion.Text = $"v{Application.ProductVersion}";
            this.lnkGiteePage.Links.Add(new LinkLabel.Link() { LinkData = "https://gitee.com/dWwwang/SiMayRemoteMonitorOS" });
            this.lnkCredits.Links.Add(new LinkLabel.Link() { LinkData = "https://gitee.com/dWwwang/SiMayRemoteMonitorOS/blob/master/LICENSE" });
        }

        private void LnkCredits_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }
    }
}
