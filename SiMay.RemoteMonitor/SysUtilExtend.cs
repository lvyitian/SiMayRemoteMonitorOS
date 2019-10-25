using SiMay.RemoteMonitor.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace SiMay.RemoteMonitor
{
    public class SysUtilExtend
    {
        public static Image GetResourceImageByName(string name) 
            => Resources.ResourceManager.GetObject(name, Resources.Culture) as Image;
    }
}
