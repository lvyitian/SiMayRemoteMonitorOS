using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Net.SessionProviderService.OnChannelListViewItem
{
    public class ChannelListViewItem : ListViewItem
    {
        public ChannelListViewItem(TcpChannelContext context)
        {
            TcpChannelContext = context;
        }
        public TcpChannelContext TcpChannelContext { get; set; }
    }
}
