using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public static class SimpleApplicationHelper
    {
        public static IDictionary<string, SimpleApplicationBase> SimpleApplicationCollection = new Dictionary<string, SimpleApplicationBase>();

        /// <summary>
        /// 链式简单程序注册方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="simpleApplicationCollection"></param>
        /// <returns></returns>
        public static IDictionary<string, SimpleApplicationBase> SimpleApplicationRegister<T>(this IDictionary<string, SimpleApplicationBase> simpleApplicationCollection)
            where T : SimpleApplicationBase, new()
        {
            simpleApplicationCollection[typeof(T).FullName] = Activator.CreateInstance<T>();
            return simpleApplicationCollection;
        }

        public static T GetSimpleApplication<T>(this IDictionary<string, SimpleApplicationBase> simpleApplicationCollection)
            where T : SimpleApplicationBase, new()
            => simpleApplicationCollection[typeof(T).FullName].ConvertTo<T>();

    }
}
