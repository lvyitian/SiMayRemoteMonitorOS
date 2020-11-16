using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public static class SimpleServiceHelper
    {
        public static IDictionary<string, RemoteSimpleServiceBase> SimpleServiceCollection = new Dictionary<string, RemoteSimpleServiceBase>();

        public static IDictionary<int, RemoteSimpleServiceBase> SimpleServiceTargetHeadMaping = new Dictionary<int, RemoteSimpleServiceBase>();
        /// <summary>
        /// 链式简单程序注册方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="simpleServiceCollection"></param>
        /// <returns></returns>
        public static IDictionary<string, RemoteSimpleServiceBase> SimpleServiceRegister<T>(this IDictionary<string, RemoteSimpleServiceBase> simpleServiceCollection)
            where T : RemoteSimpleServiceBase, new()
        {
            var instance = Activator.CreateInstance<T>();
            simpleServiceCollection[typeof(T).FullName] = instance;

            var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault();
                if (attr.IsNull())
                    continue;

                var messageHead = attr.ConvertTo<PacketHandler>().MessageHead.ConvertTo<int>();
                SimpleServiceTargetHeadMaping[messageHead] = instance;
            }


            return simpleServiceCollection;
        }

        public static T GetSimpleService<T>(this IDictionary<string, RemoteSimpleServiceBase> simpleServiceCollection)
            where T : RemoteSimpleServiceBase, new()
            => simpleServiceCollection[typeof(T).FullName].ConvertTo<T>();

    }
}
