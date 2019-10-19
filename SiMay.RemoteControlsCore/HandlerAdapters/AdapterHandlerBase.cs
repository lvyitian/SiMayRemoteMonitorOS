using SiMay.Core;
using SiMay.Net.SessionProvider.SessionBased;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public abstract class AdapterHandlerBase : IDisposable
    {
        /// <summary>
        /// 用户状态上下文
        /// </summary>
        public object StateContext { get; set; }

        /// <summary>
        /// 重连AppKey
        /// </summary>
        public string ResetAppKey { get; set; }

        /// <summary>
        /// 来源备注名
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// 被控服务身份Id+AppKey
        /// </summary>
        public string IdentifyId { get; set; }

        /// <summary>
        /// 当前会话是否关闭
        /// </summary>
        public bool IsClose { get; private set; }

        /// <summary>
        /// 展示应用对象
        /// </summary>
        public IApplication App { get; set; }

        /// <summary>
        /// 当前会话对象
        /// </summary>
        public SessionHandler Session { get; set; }

        /// <summary>
        /// 当会话重连后触发
        /// </summary>
        /// <param name="session"></param>
        internal virtual void ContinueTask(SessionHandler session)
            => App.ContinueTask(this);

        /// <summary>
        /// 当会话中断后触发
        /// </summary>
        /// <param name="session"></param>
        internal virtual void SessionClosed(SessionHandler session)
            => App.SessionClose(this);

        /// <summary>
        /// 数据接收时触发(底层接收数据,未解压)
        /// </summary>
        /// <param name="session"></param>
        internal virtual void MessageReceive(SessionHandler session)
        { }

        /// <summary>
        /// 数据接收完成
        /// </summary>
        /// <param name="session"></param>
        internal abstract void MessageReceived(SessionHandler session);


        /// <summary>
        /// 使用当前会话发送实体对象
        /// </summary>
        /// <returns></returns>
        public void SendAsyncMessage(MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            Session.SendAsync(bytes);
        }

        /// <summary>
        /// 使用当前会话仅发送消息头或者数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        public void SendAsyncMessage(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            Session.SendAsync(bytes);
        }

        /// <summary>
        /// 使用当前会话发送字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lpString"></param>
        public void SendAsyncMessage(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            Session.SendAsync(bytes);
        }


        /// <summary>
        /// 断开当前会话
        /// </summary>
        /// <param name="session"></param>
        public virtual void CloseHandler()
        {
            this.IsClose = true;
            SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        public void Dispose()
        {
            this.CloseHandler();
        }
    }
}
