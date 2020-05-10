using SiMay.Basic;
using SuperSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public class TokenHelper
    {

        /// <summary>
        /// 登录用户
        /// </summary>
        public static string Id { get; set; }

        /// <summary>
        /// 登录会话
        /// </summary>
        public static WebSocketSession Session { get; set; }


        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="session"></param>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsLegalUser(string id, string key)
        {
            //if (!_tokenDicts.ContainsKey(id))
            //{
            //    _tokenDicts[id] = new Token()
            //    {
            //        Id = id,
            //        Session = session
            //    };
            //    return true;
            //}
            //else
            //    return false;

            //if (_tokenDicts.Count > 0)
            //    return false;
            //else
            //{
            //    _tokenDicts[id]
            //    return true;
            //}

            return Session.IsNull();
        }

        /// <summary>
        /// 是否登录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Has()
        {
            return !Session.IsNull() && !Id.IsNull();
        }

        /// <summary>
        /// 注销
        /// </summary>
        public static void LogOut()
        {
            Session = null;Id = null;
        }
    }
}
