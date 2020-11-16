using SiMay.Core;
using SiMay.RemoteControls.Core;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UnconventionalApplication
{
    [Rank(100)]
    [ApplicationName("远程更新")]
    //[Application(typeof(RemoteUpdateAdapterHandler), ApplicationKeyConstant.REMOTE_UPDATE, 40)]
    public class RemoteUpdateApplication : ListViewItem, IApplication
    {
        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            throw new NotImplementedException();
        }

        public void SessionClose(ApplicationBaseAdapterHandler handler)
        {
            throw new NotImplementedException();
        }

        public void SetParameter(object arg)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
