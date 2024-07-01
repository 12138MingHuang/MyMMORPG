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
            // 如果有更多的消息类型，继续在这里注册处理器
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

#region 消息处理接口

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
}

#endregion

#region 消息处理器

namespace Network
{
    #region 响应消息处理器

    /// <summary>
    /// 响应消息处理器，处理特定类型的响应消息。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class NetMessageResponseHandler<T> : IMessageHandler<T, SkillBridge.Message.NetMessageResponse>
    {
        /// <summary>
        /// 处理响应消息的方法。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">响应消息对象</param>
        public void Handle(T sender, SkillBridge.Message.NetMessageResponse message)
        {
            this.HandleMessage(sender, message);
        }

        /// <summary>
        /// 处理响应消息的具体逻辑。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">响应消息对象</param>
        private void HandleMessage(T sender, SkillBridge.Message.NetMessageResponse message)
        {
            // 处理响应消息的具体逻辑
            Log.Info($"正在处理来自 {sender} 的响应消息");

            //// 使用反射动态处理消息
            //this.HandleMessageByReflection(sender, message);

            // 具体的处理逻辑在这里实现
            if (message.MyFirstResponse != null)
            {
                MessageDistributer<T>.Instance.RaiseEvent(sender, message.MyFirstResponse);
            }
        }

        /// <summary>
        /// 使用反射动态处理消息，将非空属性的值作为事件类型参数，通过消息分发器触发事件。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">响应消息对象</param>
        private void HandleMessageByReflection(T sender, SkillBridge.Message.NetMessageResponse message)
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

    #region 请求消息处理器

    /// <summary>
    /// 请求消息处理器，处理特定类型的请求消息。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class NetMessageRequestHandler<T> : IMessageHandler<T, SkillBridge.Message.NetMessageRequest>
    {
        /// <summary>
        /// 处理请求消息的方法。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">请求消息对象</param>
        public void Handle(T sender, SkillBridge.Message.NetMessageRequest message)
        {
            this.HandleMessage(sender, message);
        }

        /// <summary>
        /// 处理请求消息的具体逻辑。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">请求消息对象</param>
        private void HandleMessage(T sender, SkillBridge.Message.NetMessageRequest message)
        {
            // 处理请求消息的具体逻辑
            Log.Info($"正在处理来自 {sender} 的请求消息");

            //// 使用反射动态处理消息
            //this.HandleMessageByReflection(sender, message);

            // 具体的处理逻辑在这里实现
            if (message.MyFirstRequest != null)
            {
                MessageDistributer<T>.Instance.RaiseEvent(sender, message.MyFirstRequest);
            }
        }

        /// <summary>
        /// 使用反射动态处理消息，将非空属性的值作为事件类型参数，通过消息分发器触发事件。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">请求消息对象</param>
        private void HandleMessageByReflection(T sender, SkillBridge.Message.NetMessageRequest message)
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

