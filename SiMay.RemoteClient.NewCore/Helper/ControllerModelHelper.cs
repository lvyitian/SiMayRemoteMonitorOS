using SiMay.Core;
using System;
using System.Collections.Generic;
using SiMay.Basic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SiMay.Serialize.Standard;

namespace SiMay.ServiceCore
{
    public class ControllerModelHelper
    {
        public static object Invoker(string route, string packetTypeFullName, byte[] data)
        {
            var controllerName = route.Split('/')[0];
            var targetMethodName = route.Split('/')[1];

            IList<object> parameters = new List<object>();
            if (packetTypeFullName.IsNullOrEmpty())
            {
                var type = Type.GetType(packetTypeFullName);
                if (type.IsNull())
                    return null;
                var messageHelperType = typeof(MessageHelper);
                var parameterTypes = new Type[] { typeof(byte[]) };
                var method = messageHelperType.GetMethod("GetMessageEntity", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
                parameters.Add(method.MakeGenericMethod(type).Invoke(null, new object[] { data }));
            }

            if (SysUtil.ControllerTypes.Any(c => c.Name.Equals(controllerName, StringComparison.OrdinalIgnoreCase)))
            {
                var controllerTypeItem = SysUtil.ControllerTypes.First(c => c.Name.Equals(controllerName, StringComparison.OrdinalIgnoreCase));
                var targetMethod = controllerTypeItem.PublicMethodInfos.FristOrDefault(c => c.GetParameters().Count() == parameters.Count);

                if (!targetMethod.IsNull())
                {
                    var instance = Activator.CreateInstance(controllerTypeItem.Type);
                    var returnInstance = targetMethod.Invoke(instance, parameters.ToArray());
                    if (targetMethod.ReturnParameter.ParameterType != typeof(void))
                        return returnInstance;
                }
            }

            return null;
        }
    }
}
