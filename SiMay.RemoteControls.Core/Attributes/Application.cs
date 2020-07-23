using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    /// <summary>
    /// 应用属性
    /// </summary>
    public class ApplicationAttribute : Attribute
    {
        public Type ApplicationHandlerAdapterType { get; set; }

        public ApplicationAttribute(Type type) => ApplicationHandlerAdapterType = type;
    }
}
