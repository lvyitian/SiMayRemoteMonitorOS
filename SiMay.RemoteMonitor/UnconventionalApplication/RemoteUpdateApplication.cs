using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SiMay.RemoteControlsCore;
using SiMay.Core;
using SiMay.Basic;
using System.IO;

namespace SiMay.RemoteMonitor.UnconventionalApplication
{
    [ApplicationName("远程更新")]
    [UnconventionalApp]
    [Application(typeof(FileTransmissionAdapterHandler), AppJobConstant.REMOTE_REMOTE_UPDATE, 40)]
    public partial class RemoteUpdateApplication : UserControl, IApplication
    {

        [ApplicationAdapterHandler]
        public FileTransmissionAdapterHandler FileTransmissionAdapterHandler { get; set; }

        private Panel _updateListDocker;

        public void ContinueTask(ApplicationAdapterHandler handler) { }

        public void SessionClose(ApplicationAdapterHandler handler)
        {
            FileTransmissionAdapterHandler.TransmissionSendEventHandler -= TransmissionSendEventHandler;
            FileTransmissionAdapterHandler.TransmissionEndEventHandler -= TransmissionEndEventHandler;
            _updateListDocker.Controls.Remove(this);
        }

        public void SetParameter(object arg) => _updateListDocker = arg.ConvertTo<Panel>();

        public void Start()
        {
            originBox.Text = FileTransmissionAdapterHandler.OriginName;
            FileTransmissionAdapterHandler.TransmissionSendEventHandler += TransmissionSendEventHandler;
            FileTransmissionAdapterHandler.TransmissionEndEventHandler += TransmissionEndEventHandler;
            _updateListDocker.Controls.Add(this);
        }

        public void StartTransimssoinFile(string fileName, string name)
        {
            try
            {
                var fileStream = new WindowsForFileStream(new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite));

                FileTransmissionAdapterHandler.SendFileStream(fileStream, name);
            }
            catch (Exception ex)
            {
                LogHelper.WriteErrorByCurrentMethod(ex);
            }
        }
        private void TransmissionSendEventHandler(FileTransmissionAdapterHandler adapterHandler, long sendSize, long totalSize)
        {
            this.fileProgressBar.Value = (int)(totalSize / sendSize * 100);
            this.progressText.Text = $"({sendSize / 1024}/{totalSize / 1024})KB";
        }

        private void TransmissionEndEventHandler(FileTransmissionAdapterHandler adapterHandler, string fileName)
        {
            FileTransmissionAdapterHandler.InvokerController("UpdateController/StartUpdate",
                new RemoteUpdatePacket()
                {
                    FileName = fileName
                });
        }
    }
}
