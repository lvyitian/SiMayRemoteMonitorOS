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

namespace SiMay.RemoteControls.Core
{
    public abstract class MainApplicationBaseAdapterHandler : ApplicationProtocolAdapterHandler
    {
        private Dictionary<string, byte[]> _serviceCOMPlugins = new Dictionary<string, byte[]>();

        /// <summary>
        /// 简单程序集合
        /// </summary>
        public IDictionary<string, SimpleApplicationBase> SimpleApplicationCollection
            => SimpleApplicationHelper.SimpleApplicationCollection;

        public MainApplicationBaseAdapterHandler()
        {
            string[] pluginFileNames = new string[]
            {
                    "SiMay.Service.Core.dll",
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
        /// 网络传输核心库
        /// </summary>
        /// <param name="session"></param>
        protected void SendToAssemblyCoreFile(SessionProviderContext session)
        {
            session.SendTo(MessageHead.S_GLOBAL_PLUGIN,
                new ServiceAssemblyCorePluginPacket()
                {
                    Files = _serviceCOMPlugins.Select(c => new AssemblyFileItem() { AssemblyName = c.Key, Data = c.Value }).ToArray()
                });
        }
    }
}
