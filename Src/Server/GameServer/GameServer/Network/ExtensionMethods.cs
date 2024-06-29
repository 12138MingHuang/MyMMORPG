using System;
using System.Net.Sockets;

namespace GameServer.Network
{
    /// <summary>
    /// 表示最新版本 .NET 中的 Socket 异步方法。
    /// </summary>
    /// <param name="args">用于方法的 SocketAsyncEventArgs 实例。</param>
    /// <returns>如果操作异步完成则返回 true，否则返回 false。</returns>
    public delegate bool SocketAsyncMethod(SocketAsyncEventArgs args);

    /// <summary>
    /// 包含用于处理最新版本 .NET 中的 Socket 异步方法的辅助方法。
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// 扩展方法，用于简化最新版本 .NET 中的 Socket 异步方法所需的模式。
        /// </summary>
        /// <param name="socket">此方法作用的 socket 实例。</param>
        /// <param name="method">要调用的异步方法。</param>
        /// <param name="callback">方法的回调函数。注意：Completed 事件必须已附加到相同的回调函数。</param>
        /// <param name="args">用于此调用的 SocketAsyncEventArgs 实例。</param>
        public static void InvokeAsyncMethod(this Socket socket, SocketAsyncMethod method, EventHandler<SocketAsyncEventArgs> callback, SocketAsyncEventArgs args)
        {
            // 如果异步方法没有立即完成，直接调用回调函数
            if (!method(args))
            {
                callback(socket, args);
            }
        }
    }
}
