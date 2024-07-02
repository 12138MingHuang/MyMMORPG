#region 具体消息处理器

namespace Network
{
    #region 响应消息处理器

    /// <summary>
    /// 响应消息处理器，处理特定类型的响应消息。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class NetMessageResponseHandler<T> : MessageHandlerBase<T, SkillBridge.Message.NetMessageResponse>
    {
        /// <summary>
        /// 处理响应消息的具体逻辑。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">响应消息对象</param>
        protected override void HandleMessage(T sender, SkillBridge.Message.NetMessageResponse message)
        {
            // 具体的处理逻辑在这里实现
            if (message.MyFirstResponse != null)
            {
                MessageDistributer<T>.Instance.RaiseEvent(sender, message.MyFirstResponse);
            }
        }
    }

    #endregion

    #region 请求消息处理器

    /// <summary>
    /// 请求消息处理器，处理特定类型的请求消息。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class NetMessageRequestHandler<T> : MessageHandlerBase<T, SkillBridge.Message.NetMessageRequest>
    {
        /// <summary>
        /// 处理请求消息的具体逻辑。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">请求消息对象</param>
        protected override void HandleMessage(T sender, SkillBridge.Message.NetMessageRequest message)
        {
            // 具体的处理逻辑在这里实现
            if (message.MyFirstRequest != null)
            {
                MessageDistributer<T>.Instance.RaiseEvent(sender, message.MyFirstRequest);
            }
        }
    }

    #endregion
}

#endregion
