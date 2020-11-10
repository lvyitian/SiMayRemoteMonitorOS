using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public enum MOUSEKEY_KIND : byte
    {
        /// <summary>
        /// 鼠标移动
        /// </summary>
        Move,

        /// <summary>
        /// 左键按下
        /// </summary>
        LeftDown,

        /// <summary>
        /// 左键抬起
        /// </summary>
        LeftUp,

        MiddleDown,
        MiddleUp,

        /// <summary>
        /// 右键按下
        /// </summary>
        RightDown,

        /// <summary>
        /// 右键抬起
        /// </summary>
        RightUp,

        /// <summary>
        /// 滚轮
        /// </summary>
        Wheel,

        /// <summary>
        /// 按键按下
        /// </summary>
        KeyDown,

        /// <summary>
        /// 按键抬起
        /// </summary>
        KeyUp
    }
}
