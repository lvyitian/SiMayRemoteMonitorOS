using SiMay.RemoteControlsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.MainApplication
{
    public class ViewCarouselContext
    {
        /// <summary>
        /// 轮播间隔
        /// </summary>
        public int ViewCarouselInterval { get; set; }

        /// <summary>
        /// 视图行
        /// </summary>
        public int ViewRow { get; set; }

        /// <summary>
        /// 视图列
        /// </summary>
        public int ViewColum { get; set; }

        /// <summary>
        /// 停留视图
        /// </summary>
        public IList<IDesktopView> AlwaysViews { get; set; }

        /// <summary>
        /// 是否启用轮播
        /// </summary>
        public bool Enabled { get; set; }

        public ViewCarouselContext()
        {
            AlwaysViews = new List<IDesktopView>();
        }
    }
}
