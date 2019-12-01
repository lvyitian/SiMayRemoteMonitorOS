using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public class DisableAttribute : Attribute
    {
        /// <summary>
        /// 停用应用
        /// </summary>
        public DisableAttribute()
        {
        }
    }
}
