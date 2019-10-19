using SiMay.Core;
using SiMay.Core.Packets;
using SiMay.Net.SessionProvider.SessionBased;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public static class ServiceCOMLoader
    {
        private static Dictionary<string, byte[]> ServiceCOMPlugins = new Dictionary<string, byte[]>();

        static ServiceCOMLoader()
        {
            //string[] pluginFileNames = new string[]
            //{
            //    "SiMayServiceCore.dll",
            //    "SiMay.Core.dll",
            //    "SiMay.Serialize.dll",
            //    "SiMay.Basic.dll",
            //    "AForge.Video.dll",
            //    "AForge.Video.DirectShow.dll"
            //};

            //foreach (var fileName in pluginFileNames)
            //{
            //    var path = Path.Combine(Environment.CurrentDirectory, "plugins", fileName);
            //    if (File.Exists(path))
            //        ServiceCOMPlugins.Add(fileName, File.ReadAllBytes(path));
            //    else
            //        throw new FileNotFoundException("服务插件缺失:" + fileName);
            //}
        }

        /// <summary>
        /// 主连接数据发送函数，使未加载服务插件的被控端加载插件,注意!!非主连接勿用
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data"></param>
        internal static void SendMessageDoHasCOM(this SessionHandler session, byte[] data)
        {
            //var syncContext = session.AppTokens[SysConstants.INDEX_WORKER] as SessionSyncContext;
            //if (!syncContext.HasLoadServiceCOM)
            //    SendServicePlugins(session);
            session.SendAsync(data);
        }

        /// <summary>
        /// 发送并加载插件
        /// </summary>
        /// <param name="session"></param>
        private static void SendServicePlugins(SessionHandler session)
        {
            byte[] data = MessageHelper.CopyMessageHeadTo(
                MessageHead.S_MAIN_PLUGIN_FILES,
                new ServicePluginPack()
                {
                    Files = ServiceCOMPlugins.Select(c => new PluginItem() { FileName = c.Key, PayLoad = c.Value }).ToArray()
                });
            session.SendAsync(data);
        }
    }
}
