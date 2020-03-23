using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteControlsCore;
using SiMay.Serialize.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainApplication
{

    public partial class BuilderServiceForm : Form
    {

        public BuilderServiceForm()
        {
            InitializeComponent();
        }

        private IDictionary<string, string> localHosts = new Dictionary<string, string>();

        private void button1_Click(object sender, EventArgs e)
        {

            if (mls_address.Text == "" || mls_port.Text == "")
            {
                MessageBoxHelper.ShowBoxExclamation("请输入完整正确的上线信息,否则可能造成上线失败!");
                return;
            }
            logList.Items.Add("正在发起连接测试!");
            Socket testSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                testSock.Connect(HostHelper.GetHostByName(mls_address.Text), int.Parse(mls_port.Text));
                testSock.Close();
                MessageBoxHelper.ShowBoxExclamation("连接: " + mls_address.Text + ":" + mls_port.Text + " 成功!", "连接成功");
            }
            catch
            {
                MessageBoxHelper.ShowBoxError("连接: " + mls_address.Text + ":" + mls_port.Text + " 失败!", "连接失败");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {

            if (mls_address.Text.IsNullOrEmpty() || mls_port.Text.IsNullOrEmpty() || txtInitName.Text.IsNullOrEmpty() || txtAccesskey.Text.IsNullOrEmpty() || groupNameBox.Text.IsNullOrEmpty())
            {
                MessageBoxHelper.ShowBoxExclamation("请输入完整正确的上线信息,否则可能造成上线失败!");
                return;
            }

            if (groupNameBox.Text == "全部")
            {
                MessageBoxHelper.ShowBoxExclamation("分组名不能'全部'!");
                return;
            }

            logList.Items.Clear();

            logList.Items.Add("配置信息初始化..");

            var autoRun = false;
            var svcInstall = false;

            if (installMode.SelectedIndex == 1)
                autoRun = true;
            else if (installMode.SelectedIndex == 2)
                svcInstall = true;

            var options = new ServiceOptions()
            {
                Id = Guid.NewGuid().ToString(),
                Host = mls_address.Text,
                Port = int.Parse(mls_port.Text),
                Remark = txtInitName.Text,
                AccessKey = int.Parse(txtAccesskey.Text),
                IsHide = ishide.Checked,
                IsAutoRun = autoRun,
                IsMutex = mutex.Checked,
                InstallService = svcInstall,
                ServiceName = "SiMayService",
                ServiceDisplayName = "SiMay远程被控服务",
                SessionMode = sessionModeList.SelectedIndex,
                GroupName = groupNameBox.Text
            };
            string name = "SiMayService.exe";

            string datfileName = Path.Combine(Environment.CurrentDirectory, "dat", name);

            logList.Items.Add("准备将配置信息写入文件中");

            if (!File.Exists(datfileName))
            {
                logList.Items.Add("配置文件不存在.");
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "可执行文件|*.exe";
            dlg.Title = "生成";
            dlg.FileName = "SiMayService";
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                logList.Items.Add("配置信息写入被终止了!");
                return;
            }
            if (dlg.FileName != "")
            {
                logList.Items.Add("配置信息写入中...");
                var optionsBytes = PacketSerializeHelper.SerializePacket(options);
                bool err = this.WirteOptions(optionsBytes, datfileName, dlg.FileName);

                if (err != true)
                {
                    logList.Items.Add("配置信息写入失败,请检查配置文件是否被占用!");
                    return;
                }

                logList.Items.Add("配置信息写入成功!");
            }
            else
            {
                logList.Items.Add("配置信息写入被终止了!");
                return;
            }
            MessageBoxHelper.ShowBoxExclamation("服务端文件已生成到位置:" + dlg.FileName);

            this.Close();
        }

        public bool WirteOptions(byte[] options, string sourcefileName, string fileName)
        {
            //追加位置写入
            try
            {

                byte[] Bytes = new byte[sizeof(Int16) + sizeof(Int32) + options.Length];//长度加标识
                options.CopyTo(Bytes, 0);
                BitConverter.GetBytes(options.Length).CopyTo(Bytes, options.Length);
                BitConverter.GetBytes((short)9999).CopyTo(Bytes, options.Length + sizeof(Int32));

                byte[] SourceFileData = File.ReadAllBytes(sourcefileName);
                FileStream fs = new FileStream(fileName, FileMode.Create);
                fs.Write(SourceFileData, 0, SourceFileData.Length);
                fs.Seek(0, SeekOrigin.End);
                fs.Write(Bytes, 0, Bytes.Length);
                fs.Flush();
                fs.Close();
                fs.Dispose();
                return true;
            }
            catch
            {
                return false;
            }

            //寻位写入
            //try
            //{
            //    int location = 0;
            //    byte[] fileBytes = File.ReadAllBytes(sourcefileName);
            //    for (int i = 0; i < fileBytes.Length; i++)
            //    {
            //        if (i + 514 <= fileBytes.Length)
            //        {
            //            if (fileBytes[i] == 91 && fileBytes[i + 514] == 93)
            //            {
            //                location = i;
            //                break;
            //            }
            //        }
            //    }

            //    byte[] b = System.IO.File.ReadAllBytes(sourcefileName);
            //    byte[] value = Encoding.Unicode.GetBytes(ReplaceString);
            //    FileStream fs = new FileStream(fileName, FileMode.Create);
            //    fs.Write(b, 0, b.Length);
            //    fs.Seek(location, SeekOrigin.Begin);
            //    fs.Write(value, 0, value.Length);
            //    fs.Seek(location + value.Length, SeekOrigin.Begin);
            //    byte[] by = new byte[514 - value.Length];
            //    for (int i = 0; i < by.Length; i++)
            //    {
            //        by[i] = 0x00;
            //    }
            //    fs.Write(by, 0, by.Length);
            //    fs.Flush();
            //    fs.Close();
            //    fs.Dispose();
            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }

        private void BuildClientForm_Load(object sender, EventArgs e)
        {
            txtAccesskey.Text = AppConfiguration.AccessKey.ToString();
            string strHosts = AppConfiguration.LHostString;

            int index = int.Parse(AppConfiguration.SessionMode);
            sessionModeList.SelectedIndex = index;

            installMode.Text = installMode.Items[0].ToString();

            string[] strarrays = strHosts.Split(',');

            for (int i = 0; i < strarrays.Length - 1; i++)
            {
                string[] strs = strarrays[i].Split(':');

                if (!localHosts.ContainsKey(strs[0]))
                {
                    localHosts.Add(strs[0], strs[1]);
                    mls_address.Items.Add(strs[0]);
                    mls_port.Items.Add(strs[1]);
                }
                else
                    logList.Items.Add("..配置文件存在重复域名!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (mls_address.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowBoxExclamation("请选中地址记录", "提示");
                return;
            }
            if (MessageBox.Show("确定该地址记录吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
            {
                localHosts.Remove(mls_address.Items[mls_address.SelectedIndex].ToString());
                mls_address.Items.RemoveAt(mls_address.SelectedIndex);
                mls_port.Items.RemoveAt(mls_port.SelectedIndex);
                SaveAddressInfo();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mls_address.Text == "" || mls_port.Text == "" || txtInitName.Text == "")
            {
                MessageBoxHelper.ShowBoxExclamation("请输入完整正确的上线信息,否则可能造成上线失败!");
                return;
            }
            if (!localHosts.ContainsKey(mls_address.Text))
            {
                localHosts.Add(mls_address.Text, mls_port.Text);
                mls_address.Items.Add(mls_address.Text);
                mls_port.Items.Add(mls_port.Text);
            }
            else
            {
                logList.Items.Add("记录已存在!保存失败");
            }

            SaveAddressInfo();
        }

        private void SaveAddressInfo()
        {
            AppConfiguration.LHostString = string.Join(",", localHosts.Select(c => c.Key + ":" + c.Value).ToArray()); ;
            logList.Items.Add("记录已保存");

        }
        private void mls_address_SelectedIndexChanged(object sender, EventArgs e)
        {
            mls_port.Text = localHosts[mls_address.Text];
        }
    }
}