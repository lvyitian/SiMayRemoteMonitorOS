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
        public int Rank { get; set; }
        public Type AppHandlerAdapterType { get; set; }
        public string AppKey { get; set; }
        public ApplicationAttribute(Type type, string appKey, int rank)
        {
            AppKey = appKey;
            AppHandlerAdapterType = type;
            Rank = rank;
        }
    }
}
