using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService
{
    public class AppConfiguartion
    {
        public static string GroupName
        {
            get
            {
                var groupName = RegValueHelper.GetConfig("GroupName");
                return groupName == "" ? null : groupName;
            }
            set
            {
                RegValueHelper.SetConfig("GroupName", value);
            }
        }

        public static string RemarkInfomation
        {
            get
            {
                var remark = RegValueHelper.GetConfig("RemarkInfomation");
                return remark;
            }
            set
            {
                RegValueHelper.SetConfig("RemarkInfomation", value);
            }
        }

        public static string IsOpenScreenView
        {
            get
            {
                return RegValueHelper.GetConfig("isOpenScreenView");
            }
            set
            {
                RegValueHelper.SetConfig("isOpenScreenView", value);
            }
        }

        public static string IsScreenRecord
        {
            get
            {
                return RegValueHelper.GetConfig("IsScreenRecord");
            }
            set
            {
                RegValueHelper.SetConfig("IsScreenRecord", value);
            }
        }

        public static string ScreenRecordHeight
        {
            get
            {
                return RegValueHelper.GetConfig("ScreenRecordHeight");
            }
            set
            {
                RegValueHelper.SetConfig("ScreenRecordHeight", value);
            }
        }
        public static string ScreenRecordWidth
        {
            get
            {
                return RegValueHelper.GetConfig("ScreenRecordWidth");
            }
            set
            {
                RegValueHelper.SetConfig("ScreenRecordWidth", value);
            }
        }
        public static string ScreenRecordSpanTime
        {
            get
            {
                return RegValueHelper.GetConfig("ScreenRecordSpanTime");
            }
            set
            {
                RegValueHelper.SetConfig("ScreenRecordSpanTime", value);
            }
        }
    }
}
