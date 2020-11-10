using SiMay.Net.SessionProviderServiceCore;
using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.SessionProviderService
{
    public class ChannelViewItem : ListViewItem
    {
        private ListViewSubItem _viewSubItem;
        public ChannelViewItem(TcpSessionChannelDispatcher channelDispatcher)
        {
            _viewSubItem = new ListViewSubItem(this, "0.00/0.00");
            this.Text = channelDispatcher.DispatcherId.ToString();
            this.SubItems.Add(DateTime.Now.ToString());
            this.SubItems.Add(channelDispatcher.ConnectionWorkType.GetDescription());
            this.SubItems.Add(_viewSubItem);
            ChannelDispatcher = channelDispatcher;
            channelDispatcher.SendStreamLengthEventHandler += SendStreamLengthEventHandler;
            channelDispatcher.ReceiveStreamLengthEventHandler += ReceiveStreamLengthEventHandler;
        }

        private void ReceiveStreamLengthEventHandler(TcpSessionChannelDispatcher channelDispatcher, long length) => ReceiveStreamLength += length;

        private void SendStreamLengthEventHandler(TcpSessionChannelDispatcher channelDispatcher, long length) => SendStreamLength += length;

        /// <summary>
        /// 发送长度
        /// </summary>
        public long SendStreamLength { get; set; }

        /// <summary>
        /// 接受长度
        /// </summary>
        public long ReceiveStreamLength { get; set; }

        public void SetVelocityText(long s, long r)
        {
            _viewSubItem.Text = $"{((float)s / 1024).ToString("0.00")} KB/{((float)r / 1024).ToString("0.00")} KB";
        }

        public TcpSessionChannelDispatcher ChannelDispatcher { get; private set; }
    }
}
