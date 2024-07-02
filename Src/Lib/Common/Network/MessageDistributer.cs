using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Common;

namespace Network
{
    /// <summary>
    /// 消息分发器（无类型）
    /// MessageDistributer
    /// </summary>
    public class MessageDistributer : MessageDistributer<object>
    {
    }

    /// <summary>
    /// 消息分发器（泛型版本）
    /// MessageDistributer
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class MessageDistributer<T> : Singleton<MessageDistributer<T>>
    {
        /// <summary>
        /// 消息参数基类，封装了消息发送者和消息内容
        /// </summary>
        class MessageArgs
        {
            public T sender;
            public Google.Protobuf.IMessage message;
        }

        /// <summary>
        /// 消息参数类，封装了消息发送者和消息内容
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        class MessageArgs<Tm> : MessageArgs where Tm : Google.Protobuf.IMessage
        {
            public new Tm message;

            public MessageArgs(T sender, Tm message)
            {
                this.sender = sender;
                this.message = message;
                base.message = message;
            }
        }

        /// <summary>
        /// 消息队列，用于存储接收到的消息。
        /// </summary>
        private Queue<MessageArgs> messageQueue = new Queue<MessageArgs>();

        /// <summary>
        /// 消息处理器委托，定义处理消息的函数。
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public delegate void MessageHandler<Tm>(T sender, Tm message);

        /// <summary>
        /// 消息处理器字典，按消息类型存储处理器。
        /// </summary>
        private Dictionary<string, System.Delegate> messageHandlers = new Dictionary<string, System.Delegate>();

        /// <summary>
        /// 缓存每个消息类型相关的属性，用于提高反射性能。
        /// </summary>
        private static readonly Dictionary<Type, PropertyInfo[]> relevantPropertiesCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// 运行标志，用于控制消息处理器的运行状态。
        /// </summary>
        private bool Running = false;

        /// <summary>
        /// 线程事件，用于线程间同步。
        /// </summary>
        private AutoResetEvent threadEvent = new AutoResetEvent(true);

        /// <summary>
        /// 工作线程数。
        /// </summary>
        public int ThreadCount = 0;

        /// <summary>
        /// 活动线程数。
        /// </summary>
        public int ActiveThreadCount = 0;

        /// <summary>
        /// 是否抛出异常标志。
        /// </summary>
        public bool ThrowException = false;

        /// <summary>
        /// 消息分发器构造函数
        /// </summary>
        public MessageDistributer()
        {
        }

        /// <summary>
        /// 订阅消息处理器
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="messageHandler">消息处理器</param>
        public void Subscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] + messageHandler;
        }

        /// <summary>
        /// 取消订阅消息处理器
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="messageHandler">消息处理器</param>
        public void Unsubscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] - messageHandler;
        }

        /// <summary>
        /// 触发消息处理器
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="msg">消息内容</param>
        public void RaiseEvent<Tm>(T sender, Tm msg)
        {
            string key = msg.GetType().Name;
            if (messageHandlers.ContainsKey(key))
            {
                MessageHandler<Tm> handler = (MessageHandler<Tm>)messageHandlers[key];
                if (handler != null)
                {
                    try
                    {
                        handler(sender, msg);
                    }
                    catch (System.Exception ex)
                    {
                        Log.ErrorFormat("消息处理异常：内部异常：{0}，消息：{1}，来源：{2}，堆栈跟踪：{3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
                        if (ThrowException)
                            throw ex;
                    }
                }
                else
                {
                    Log.Warning($"未订阅处理程序：{msg.ToString()}");
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <typeparam name="Tm">消息类型</typeparam>
        /// <param name="sender">消息发送者</param>
        /// <param name="message">消息内容</param>
        public void ReceiveMessage<Tm>(T sender, Tm message) where Tm : class, Google.Protobuf.IMessage
        {
            // 创建消息参数对象并入队
            MessageArgs<Tm> messageArgs = new MessageArgs<Tm>(sender, message);
            this.messageQueue.Enqueue(messageArgs);

            // 设置线程事件，通知处理器有新消息
            threadEvent.Set();
        }



        /// <summary>
        /// 清空消息队列
        /// </summary>
        public void Clear()
        {
            this.messageQueue.Clear();
        }

        /// <summary>
        /// 一次性分发队列中的所有消息
        /// </summary>
        public void Distribute()
        {
            if (this.messageQueue.Count == 0)
            {
                return;
            }

            while (this.messageQueue.Count > 0)
            {
                MessageArgs package = this.messageQueue.Dequeue();
                if (package.message != null)
                {
                    //// 使用预先构建的属性列表进行消息分发
                    //Type messageType = package.message.GetType();
                    //PropertyInfo[] properties = GetRelevantProperties(messageType);
                    ////PropertyInfo[] properties = messageType.GetProperties();

                    //foreach (var property in properties)
                    //{
                    //    var value = property.GetValue(package.message, null);
                    //    if (value != null)
                    //    {
                    //        Type eventType = value.GetType();
                    //        MethodInfo method = typeof(MessageDispatch<T>).GetMethod("Dispatch");
                    //        MethodInfo genericMethod = method.MakeGenericMethod(eventType);
                    //        genericMethod.Invoke(MessageDispatch<T>.Instance, new object[] { package.sender, value });
                    //    }
                    //}

                    if (package.message != null && package.message is SkillBridge.Message.NetMessage)
                    {
                        SkillBridge.Message.NetMessageRequest request = (package.message as SkillBridge.Message.NetMessage).Request;
                        MessageDispatch<T>.Instance.Dispatch(package.sender, request);
                    }
                    else if (package.message != null && package.message is SkillBridge.Message.NetMessage)
                    {
                        SkillBridge.Message.NetMessageResponse response = (package.message as SkillBridge.Message.NetMessage).Response;
                        MessageDispatch<T>.Instance.Dispatch(package.sender, response);
                    }
                    // 可以根据需要添加其他消息类型的处理
                }
            }
        }

        /// <summary>
        /// 启动消息处理器
        /// [多线程模式]
        /// </summary>
        /// <param name="ThreadNum">工作线程数</param>
        public void Start(int ThreadNum)
        {
            this.ThreadCount = ThreadNum;
            if (this.ThreadCount < 1) this.ThreadCount = 1;
            if (this.ThreadCount > 1000) this.ThreadCount = 1000;
            Running = true;
            Log.WarningFormat("开始启动 [{0}] 个消息分发线程...", this.ThreadCount);
            for (int i = 0; i < this.ThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageDistribute));
            }
            while (ActiveThreadCount < this.ThreadCount)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 停止消息处理器
        /// [多线程模式]
        /// </summary>
        public void Stop()
        {
            Log.WarningFormat("开始结束 [{0}] 个消息分发线程...", this.ThreadCount);
            Running = false;
            this.messageQueue.Clear();
            while (ActiveThreadCount > 0)
            {
                threadEvent.Set();
            }
            Thread.Sleep(100);
        }

        /// <summary>
        /// 消息处理线程
        /// [多线程模式]
        /// </summary>
        /// <param name="stateInfo">线程状态信息</param>
        private void MessageDistribute(Object stateInfo)
        {
            Log.Warning("消息分发线程启动");
            try
            {
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);
                while (Running)
                {
                    if (this.messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne();
                        continue;
                    }

                    MessageArgs package = this.messageQueue.Dequeue();
                    if (package.message != null)
                    {
                        //// 使用预先构建的属性列表进行消息分发
                        //Type messageType = package.message.GetType();
                        //PropertyInfo[] properties = GetRelevantProperties(messageType);
                        ////PropertyInfo[] properties = messageType.GetProperties();

                        //foreach (var property in properties)
                        //{
                        //    var value = property.GetValue(package.message, null);
                        //    if (value != null)
                        //    {
                        //        Type eventType = value.GetType();
                        //        MethodInfo method = typeof(MessageDispatch<T>).GetMethod("Dispatch");
                        //        MethodInfo genericMethod = method.MakeGenericMethod(eventType);
                        //        genericMethod.Invoke(MessageDispatch<T>.Instance, new object[] { package.sender, value });
                        //    }
                        //}

                        if (package.message != null && package.message is SkillBridge.Message.NetMessage)
                        {
                            SkillBridge.Message.NetMessageRequest request = (package.message as SkillBridge.Message.NetMessage).Request;
                            MessageDispatch<T>.Instance.Dispatch(package.sender, request);
                        }
                        else if (package.message != null && package.message is SkillBridge.Message.NetMessage)
                        {
                            SkillBridge.Message.NetMessageResponse response = (package.message as SkillBridge.Message.NetMessage).Response;
                            MessageDispatch<T>.Instance.Dispatch(package.sender, response);
                        }
                        // 可以根据需要添加其他消息类型的处理
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.ErrorFormat("消息分发线程异常：{0}", ex.Message);
                Log.ErrorFormat("消息分发线程异常：{0}，消息：{1}，来源：{2}，堆栈跟踪：{3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
            }
            finally
            {
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
                Log.Warning("消息分发线程结束");
            }
        }

        /// <summary>
        /// 获取特定消息类型的相关属性，排除不需要的属性并进行缓存。
        /// 过滤掉类型为 Google.Protobuf.MessageParser 和 Google.Protobuf.Reflection.MessageDescriptor 的属性
        /// </summary>
        /// <param name="messageType">消息类型。</param>
        /// <returns>相关属性的数组。</returns>
        private PropertyInfo[] GetRelevantProperties(Type messageType)
        {
            if (!relevantPropertiesCache.TryGetValue(messageType, out var properties))
            {
                properties = messageType.GetProperties()
                    .Where(p =>
                        p.PropertyType != typeof(Google.Protobuf.Reflection.MessageDescriptor) &&
                        !(p.PropertyType.IsGenericType &&
                          p.PropertyType.GetGenericTypeDefinition() == typeof(Google.Protobuf.MessageParser<>)))
                    .ToArray();
                relevantPropertiesCache[messageType] = properties;
            }
            return properties;
        }

    }
}
