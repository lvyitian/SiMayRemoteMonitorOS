using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.Entitys
{
    using System;
    using System.Collections;
    using System.Windows.Forms;
    namespace Common
    {
        /// <summary>
        /// 对ListView点击列标题自动排序功能
        /// </summary>
        public class ListViewHelper
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            public ListViewHelper()
            {
                //
                // TODO: 在此处添加构造函数逻辑
                //
            }
            public static void ListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
            {
                System.Windows.Forms.ListView lv = sender as System.Windows.Forms.ListView;
                // 检查点击的列是不是现在的排序列.
                if (e.Column == (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn)
                {
                    // 重新设置此列的排序方法.
                    if ((lv.ListViewItemSorter as ListViewColumnSorter).Order == System.Windows.Forms.SortOrder.Ascending)
                    {
                        (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
                    }
                    else
                    {
                        (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
                    }
                }
                else
                {
                    // 设置排序列，默认为正向排序
                    (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn = e.Column;
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
                }
                // 用新的排序方法对ListView排序
                ((System.Windows.Forms.ListView)sender).Sort();
            }
        }
        /// <summary>
        /// 继承自IComparer
        /// </summary>
        public class ListViewColumnSorter : System.Collections.IComparer
        {
            /// <summary>
            /// 指定按照哪个列排序
            /// </summary>
            private int ColumnToSort;
            /// <summary>
            /// 指定排序的方式
            /// </summary>
            private System.Windows.Forms.SortOrder OrderOfSort;
            /// <summary>
            /// 声明CaseInsensitiveComparer类对象
            /// </summary>
            private System.Collections.CaseInsensitiveComparer ObjectCompare;
            /// <summary>
            /// 构造函数
            /// </summary>
            public ListViewColumnSorter()
            {
                // 默认按第一列排序
                ColumnToSort = 0;
                // 排序方式为不排序
                OrderOfSort = System.Windows.Forms.SortOrder.None;
                // 初始化CaseInsensitiveComparer类对象
                ObjectCompare = new System.Collections.CaseInsensitiveComparer();
            }
            /// <summary>
            /// 重写IComparer接口.
            /// </summary>
            /// <param name="x">要比较的第一个对象</param>
            /// <param name="y">要比较的第二个对象</param>
            /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
            public int Compare(object x, object y)
            {
                int compareResult;
                System.Windows.Forms.ListViewItem listviewX, listviewY;
                // 将比较对象转换为ListViewItem对象
                listviewX = (System.Windows.Forms.ListViewItem)x;
                listviewY = (System.Windows.Forms.ListViewItem)y;
                string xText = listviewX.SubItems[ColumnToSort].Text;
                string yText = listviewY.SubItems[ColumnToSort].Text;
                int xInt, yInt;
                // 比较,如果值为IP地址，则根据IP地址的规则排序。
                if (IsIP(xText) && IsIP(yText))
                {
                    compareResult = CompareIp(xText, yText);
                }
                else if (int.TryParse(xText, out xInt) && int.TryParse(yText, out yInt)) //是否全为数字
                {
                    //比较数字
                    compareResult = CompareInt(xInt, yInt);
                }
                else
                {
                    //比较对象
                    compareResult = ObjectCompare.Compare(xText, yText);
                }
                // 根据上面的比较结果返回正确的比较结果
                if (OrderOfSort == System.Windows.Forms.SortOrder.Ascending)
                {
                    // 因为是正序排序，所以直接返回结果
                    return compareResult;
                }
                else if (OrderOfSort == System.Windows.Forms.SortOrder.Descending)
                {
                    // 如果是反序排序，所以要取负值再返回
                    return (-compareResult);
                }
                else
                {
                    // 如果相等返回0
                    return 0;
                }
            }
            /// <summary>
            /// 判断是否为正确的IP地址，IP范围（0.0.0.0～255.255.255）
            /// </summary>
            /// <param name="ip">需验证的IP地址</param>
            /// <returns></returns>
            public bool IsIP(String ip)
            {
                return System.Text.RegularExpressions.Regex.Match(ip, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$").Success;
            }
            /// <summary>
            /// 比较两个数字的大小
            /// </summary>
            /// <param name="ipx">要比较的第一个对象</param>
            /// <param name="ipy">要比较的第二个对象</param>
            /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
            private int CompareInt(int x, int y)
            {
                if (x > y)
                {
                    return 1;
                }
                else if (x < y)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            /// <summary>
            /// 比较两个IP地址的大小
            /// </summary>
            /// <param name="ipx">要比较的第一个对象</param>
            /// <param name="ipy">要比较的第二个对象</param>
            /// <returns>比较的结果.如果相等返回0，如果x大于y返回1，如果x小于y返回-1</returns>
            private int CompareIp(string ipx, string ipy)
            {
                string[] ipxs = ipx.Split('.');
                string[] ipys = ipy.Split('.');
                for (int i = 0; i < 4; i++)
                {
                    if (Convert.ToInt32(ipxs[i]) > Convert.ToInt32(ipys[i]))
                    {
                        return 1;
                    }
                    else if (Convert.ToInt32(ipxs[i]) < Convert.ToInt32(ipys[i]))
                    {
                        return -1;
                    }
                    else
                    {
                        continue;
                    }
                }
                return 0;
            }
            /// <summary>
            /// 获取或设置按照哪一列排序.
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }
            /// <summary>
            /// 获取或设置排序方式.
            /// </summary>
            public System.Windows.Forms.SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }
        }
    }
}
