using SiMay.Core;
using SiMay.RemoteControlsCore;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UnconventionalApplication
{

    [ApplicationName("远程更新")]
    [Application(typeof(RemoteUpdateAdapterHandler), AppJobConstant.REMOTE_UPDATE, 40)]
    public class RemoteUpdateApplication : ListViewItem, IApplication
    {
        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            throw new NotImplementedException();
        }

        public void SessionClose(ApplicationAdapterHandler handler)
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
