using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public abstract class MainApplicationBaseAdapterHandler : ApplicationProtocolAdapterHandler
    {
        private Dictionary<string, byte[]> _serviceCOMPlugins = new Dictionary<string, byte[]>();

        public MainApplicationBaseAdapterHandler()
        {
            //    string[] pluginFileNames = new string[]
            //    {
            //        "SiMayServiceCore.dll",
            //        "SiMay.Core.dll",
            //        "SiMay.Serialize.dll",
            //        "SiMay.Basic.dll",
            //        "AForge.Video.dll",
            //        "AForge.Video.DirectShow.dll"
            //    };

            //    foreach (var fileName in pluginFileNames)
            //    {
            //        var path = Path.Combine(Environment.CurrentDirectory, "plugins", fileName);
            //        if (File.Exists(path))
            //            ServiceCOMPlugins.Add(fileName, File.ReadAllBytes(path));
            //        else
            //            throw new FileNotFoundException("服务插件缺失:" + fileName);
            //    }

        }

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="session"></param>
        private void SendServicePlugins(SessionProviderContext session)
        {
            SendTo(session, MessageHead.S_MAIN_PLUGIN_FILES,
                new ServicePluginPack()
                {
                    Files = _serviceCOMPlugins.Select(c => new PluginItem() { FileName = c.Key, PayLoad = c.Value }).ToArray()
                });
        }

        /// <summary>
        /// 发送实体对象
        /// </summary>
        /// <returns></returns>
        protected void HasComWithSendTo(SessionProviderContext session, MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            SendToBefore(session, bytes);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        protected void HasComWithSendTo(SessionProviderContext session, MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            SendToBefore(session, bytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lpString"></param>
        protected void HasComWithSendTo(SessionProviderContext session, MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            SendToBefore(session, bytes);
        }

        //protected override void SendToBefore(SessionProviderContext session, byte[] data)
        //{
        //    //var syncContext = session.AppTokens[SysConstants.INDEX_WORKER] as SessionSyncContext;
        //    //if (!syncContext[SysConstants.HasLoadServiceCOM].ConvertTo<bool>())
        //    //    SendServicePlugins(session);
        //    base.SendToBefore(session, data);
        //}
    }
}
