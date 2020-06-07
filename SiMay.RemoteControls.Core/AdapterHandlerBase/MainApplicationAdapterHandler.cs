using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.IO;
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
            string[] pluginFileNames = new string[]
            {
                    "SiMayService.Core.dll",
                    "SiMay.Core.Standard.dll",
                    "SiMay.Platform.Windows.dll",
                    "Microsoft.Win32.Registry.dll",
                    "Microsoft.Win32.Primitives.dll",
                    "AForge.Video.dll",
                    "AForge.Video.DirectShow.dll"
            };

            foreach (var fileName in pluginFileNames)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "Plugins", fileName);
                if (File.Exists(path))
                    _serviceCOMPlugins.Add(fileName, File.ReadAllBytes(path));
                else
                    throw new FileNotFoundException("服务插件缺失:" + fileName);
            }

        }

        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="session"></param>
        protected void SendServicePlugins(SessionProviderContext session)
        {
            SendTo(session, MessageHead.S_GLOBAL_PLUGIN,
                new ServicePluginPack()
                {
                    Files = _serviceCOMPlugins.Select(c => new PluginItem() { FileName = c.Key, PayLoad = c.Value }).ToArray()
                });
        }
    }
}
