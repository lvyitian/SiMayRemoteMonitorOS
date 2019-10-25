using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainApplication
{
    public partial class DesktopRecordViewerForm : Form
    {
        public DesktopRecordViewerForm()
        {
            InitializeComponent();
        }

        string _titleModel = "桌面记录查看 - 总共{0}个记录，正在查看第{1}个记录 - {2}";
        int _fileIndex = -1;
        List<string> paths = new List<string>();

        private void DesktopRecordViewer_Load(object sender, EventArgs e)
        {
            init();
        }

        private void init()
        {
            if (Directory.Exists(Environment.CurrentDirectory + "\\ScreenRecord"))
            {
                var directorys = new DirectoryInfo(Environment.CurrentDirectory + "\\ScreenRecord").GetDirectories();
                foreach (var dir in directorys)
                {
                    usersCombox.Items.Add(dir.Name);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (usersCombox.SelectedIndex < 0)
            {
                MessageBoxHelper.ShowBoxExclamation("请选择用户目录~");
                return;
            }
            paths.Clear();

            var startime = DateTime.Parse(startimeBox.Text);
            var endtime = DateTime.Parse(endtimeBox.Text);


            var files = new DirectoryInfo(Environment.CurrentDirectory + "\\ScreenRecord\\" + this.usersCombox.Text).GetFiles("*.png");
            foreach (var file in files)
            {
                long fileTime = Int64.Parse(file.Name.Replace(".png", ""));
                DateTime createTime = DateTime.FromFileTime(fileTime);

                if (createTime > startime || createTime.DayOfYear == startime.DayOfYear)//并且等于当前日期框时间
                    if (createTime < endtime || createTime.DayOfYear == endtime.DayOfYear)
                        paths.Add(file.FullName);
            }

            if (paths.Count > 0)
            {
                //重新回到第一张图片
                _fileIndex = -1;
                _fileIndex++;
                string fileName = paths[_fileIndex];
                if (File.Exists(fileName))
                {
                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        pictureBox.Image = Image.FromStream(ms);
                    }
                    this.Text = string.Format(_titleModel, paths.Count, (_fileIndex + 1), fileName);
                }

            }
            else
            {
                pictureBox.Image = null;
                this.Text = string.Format(_titleModel, 0, 0, "");
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((_fileIndex + 1) < paths.Count)
            {
                _fileIndex++;
                string fileName = paths[_fileIndex];
                if (File.Exists(fileName))
                {
                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        pictureBox.Image = Image.FromStream(ms);
                    }
                    this.Text = string.Format(_titleModel, paths.Count, (_fileIndex + 1), fileName);
                }
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("没有下一张了哦~");
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (paths.Count > (_fileIndex - 1) && (_fileIndex - 1) > -1)
            {
                _fileIndex--;
                string fileName = paths[_fileIndex];
                if (File.Exists(fileName))
                {
                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        pictureBox.Image = Image.FromStream(ms);
                    }
                    this.Text = string.Format(_titleModel, paths.Count, (_fileIndex + 1), fileName);
                }
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("没有上一张了哦~");
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_fileIndex > -1)
            {
                string path = paths[_fileIndex];
                if (File.Exists(path))
                {
                    var result = MessageBox.Show("确定删除该记录吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

                    if (result == DialogResult.OK)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                            paths.RemoveAt(_fileIndex);
                            MessageBoxHelper.ShowBoxExclamation("记录已删除!");
                        }
                    }
                }
            }
            else
            {
                MessageBoxHelper.ShowBoxExclamation("当前没有可删除的记录!");
            }
        }

        private void usersCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            paths.Clear();

            var files = new DirectoryInfo(Environment.CurrentDirectory + "\\ScreenRecord\\" + this.usersCombox.Text).GetFiles("*.png");
            foreach (var file in files)
            {
                paths.Add(file.FullName);
            }

            if (paths.Count > 0)
            {
                _fileIndex++;
                string fileName = paths[_fileIndex];
                if (File.Exists(fileName))
                {

                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        pictureBox.Image = Image.FromStream(ms);
                    }
                    this.Text = string.Format(_titleModel, paths.Count, (_fileIndex + 1), fileName);
                }

            }
            else
            {
                _fileIndex = -1;
                pictureBox.Image = null;
                this.Text = string.Format(_titleModel, 0, 0, "");
                return;
            }
        }
    }
}
