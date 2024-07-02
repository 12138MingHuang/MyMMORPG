#region 消息处理接口和抽象类

namespace Network
{
    /// <summary>
    /// 通用消息处理接口
    /// 定义了一个处理消息的通用方法，所有消息处理器都应实现此接口。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    /// <typeparam name="Tm">消息类型</typeparam>
    #region 接口定义
    public interface IMessageHandler<T, Tm> where Tm : class, Google.Protobuf.IMessage
    {
        /// <summary>
        /// 处理消息的方法
        /// </summary>
        /// <param name="sender">消息发送者，通常是网络连接或通信对象</param>
        /// <param name="message">要处理的消息对象</param>
        void Handle(T sender, Tm message);
    }
    #endregion

    #region 消息处理器抽象类

    /// <summary>
    /// 消息处理器抽象类，定义了处理消息的通用逻辑。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    /// <typeparam name="Tm">消息类型</typeparam>
    public abstract class MessageHandlerBase<T, Tm> : IMessageHandler<T, Tm> where Tm : class, Google.Protobuf.IMessage
    {
        /// <summary>
        /// 处理消息的方法。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息对象</param>
        public void Handle(T sender, Tm message)
        {
            // 具体的处理逻辑在 HandleMessage 方法中实现
            this.HandleMessage(sender, message);

            // 可选的反射处理逻辑
            // this.HandleMessageByReflection(sender, message);
        }

        /// <summary>
        /// 处理消息的具体逻辑，由子类实现。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息对象</param>
        protected abstract void HandleMessage(T sender, Tm message);

        /// <summary>
        /// 使用反射动态处理消息，将非空属性的值作为事件类型参数，通过消息分发器触发事件。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息对象</param>
        protected void HandleMessageByReflection(T sender, Tm message)
        {
            var messageType = message.GetType();
            var properties = messageType.GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(message, null);
                if (value != null)
                {
                    var eventType = value.GetType();
                    var method = typeof(MessageDistributer<T>).GetMethod("RaiseEvent").MakeGenericMethod(eventType);
                    method.Invoke(MessageDistributer<T>.Instance, new object[] { sender, value });
                }
            }
        }
    }

    #endregion
}

#endregion
