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
        /// 不启用
        /// </summary>
        public DisableAttribute()
        {
        }
    }
}
