using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
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
        //public class ControlTypeContext
        //{
        //    public int Rank { get; set; }
        //    public string DisplayName { get; set; }
        //    public string ControlKey { get; set; }
        //    public string ResourceName { get; set; }
        //    public bool ShowOnTools { get; set; }
        //    public Type CtrlType { get; set; }
        //}
        //public static List<ControlTypeContext> ControlTypes { get; set; }
        //static SysUtil()
        //{
        //    List<ControlTypeContext> controlTypes = new List<ControlTypeContext>();
        //    var types = Assembly.GetExecutingAssembly().GetTypes();
        //    foreach (var type in types)
        //    {
        //        if(typeof(IControlSource).IsAssignableFrom(type) && type.IsClass)
        //        {
        //            if (type.GetCustomAttribute<DisableAttribute>() != null)
        //                continue;
        //            var context = new ControlTypeContext()
        //            {
        //                Rank = type.GetRank(),
        //                DisplayName = type.GetControlDisplayName() ?? throw new Exception(type.Name + ":The control name cannot be empty!"),
        //                ControlKey = type.GetControlKey() ?? throw new Exception(type.Name + ":The controlKey cannot be empty!"),
        //                ResourceName = type.GetIconResourceName() ?? throw new Exception(type.Name + ":The ImageResource Name cannot be empty!"),
        //                ShowOnTools = type.ShowOnTools(),
        //                CtrlType = type
        //            };
        //            controlTypes.Add(context);
        //        }
        //    }
        //    ControlTypes = controlTypes;
        //}

        public static Image GetResourceImageByName(string name) 
            => Resources.ResourceManager.GetObject(name, Resources.Culture) as Image;
    }
}
