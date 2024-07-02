using Common;
using System;
using System.Collections.Generic;

#region 消息分发器

namespace Network
{
    /// <summary>
    /// 消息分发器
    /// 负责根据不同的消息类型分发消息到对应的处理器
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class MessageDispatch<T> : Singleton<MessageDispatch<T>>
    {
        #region 字段

        /// <summary>
        /// 存储消息处理器的字典，按消息类型(Type)存储对应的处理方法(Action)。
        /// </summary>
        private readonly Dictionary<Type, object> messageHandlers = new Dictionary<Type, object>();

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数，初始化消息处理器
        /// </summary>
        public MessageDispatch()
        {
            this.InitializeMessageHandlers();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化消息处理器，注册所有支持的消息类型和处理方法
        /// </summary>
        private void InitializeMessageHandlers()
        {
            this.RegisterHandler(new NetMessageResponseHandler<T>());
            this.RegisterHandler(new NetMessageRequestHandler<T>());

        }

        /// <summary>
        /// 注册消息处理器
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="handler">处理器实例</param>
        private void RegisterHandler<Tm>(IMessageHandler<T, Tm> handler) where Tm : class, Google.Protobuf.IMessage
        {
            this.messageHandlers[typeof(Tm)] = handler;
            Log.Info($"处理器已注册消息类型 '{typeof(Tm).Name}'");
        }

        /// <summary>
        /// 分发消息到相应的处理器
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息对象</param>
        public void Dispatch<Tm>(T sender, Tm message) where Tm : class, Google.Protobuf.IMessage
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null");
            }

            Type messageType = typeof(Tm);
            if (this.messageHandlers.TryGetValue(messageType, out var handler))
            {
                ((IMessageHandler<T, Tm>)handler).Handle(sender, message);
                Log.Info($"消息类型 '{messageType.Name}' 已分发。");
            }
            else
            {
                // 如果找不到对应的处理器，可以记录警告或者抛出异常
                Log.Warning($"未注册消息类型 '{messageType.Name}' 的处理器。");
            }
        }

        #endregion
    }
}

#endregion

