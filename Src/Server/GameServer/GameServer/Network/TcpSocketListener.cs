using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    /// <summary>
    /// TCP Socket 监听器，用于在指定的地址和端口上监听连接请求。
    /// </summary>
    public class TcpSocketListener : IDisposable
    {
        #region 字段
        private Int32 connectionBacklog; // 连接请求队列长度
        private IPEndPoint endPoint; // 监听的端点

        private Socket listenerSocket; // 监听用的 Socket
        private SocketAsyncEventArgs args; // 异步 Socket 事件参数
        #endregion

        #region 属性
        /// <summary>
        /// 获取或设置连接请求队列的长度。
        /// </summary>
        public Int32 ConnectionBacklog
        {
            get { return connectionBacklog; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("服务器运行时无法更改属性。");
                    else
                        connectionBacklog = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置要绑定监听的 IPEndPoint。
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return endPoint; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("服务器运行时无法更改属性。");
                    else
                        endPoint = value;
                }
            }
        }

        /// <summary>
        /// 获取当前服务器是否正在运行。
        /// </summary>
        public Boolean IsRunning
        {
            get { return listenerSocket != null; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 使用指定的地址、端口和连接请求队列长度，创建 TCP Socket 监听器。
        /// </summary>
        /// <param name="address">要监听的地址。</param>
        /// <param name="port">要监听的端口。</param>
        /// <param name="connectionBacklog">连接请求队列的长度。</param>
        public TcpSocketListener(String address, Int32 port, Int32 connectionBacklog)
            : this(IPAddress.Parse(address), port, connectionBacklog)
        { }

        /// <summary>
        /// 使用指定的地址、端口和连接请求队列长度，创建 TCP Socket 监听器。
        /// </summary>
        /// <param name="address">要监听的地址。</param>
        /// <param name="port">要监听的端口。</param>
        /// <param name="connectionBacklog">连接请求队列的长度。</param>
        public TcpSocketListener(IPAddress address, Int32 port, Int32 connectionBacklog)
            : this(new IPEndPoint(address, port), connectionBacklog)
        { }

        /// <summary>
        /// 使用指定的端点和连接请求队列长度，创建 TCP Socket 监听器。
        /// </summary>
        /// <param name="endPoint">要监听的端点。</param>
        /// <param name="connectionBacklog">连接请求队列的长度。</param>
        public TcpSocketListener(IPEndPoint endPoint, Int32 connectionBacklog)
        {
            this.endPoint = endPoint;

            args = new SocketAsyncEventArgs();
            args.Completed += OnSocketAccepted;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始监听 socket 连接请求。
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (!IsRunning)
                {
                    listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listenerSocket.Bind(endPoint);
                    listenerSocket.Listen(connectionBacklog);
                    BeginAccept(args);
                }
                else
                    throw new InvalidOperationException("服务器已经在运行中。");
            }
        }

        /// <summary>
        /// 停止监听 socket 连接请求。
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (listenerSocket == null)
                    return;
                listenerSocket.Close();
                listenerSocket = null;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 异步开始监听新的连接请求。
        /// </summary>
        /// <param name="args">SocketAsyncEventArgs 实例。</param>
        private void BeginAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            listenerSocket.AcceptAsync(args);
        }

        /// <summary>
        /// 当异步接受操作完成时调用。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">SocketAsyncEventArgs 实例。</param>
        private void OnSocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            SocketError error = e.SocketError;
            if (e.SocketError == SocketError.OperationAborted)
                return; // 服务器已停止

            if (e.SocketError == SocketError.Success)
            {
                Socket handler = e.AcceptSocket;
                OnSocketConnected(handler);
            }

            lock (this)
            {
                BeginAccept(e);
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 当接受到新连接时触发的事件。
        /// </summary>
        public event EventHandler<Socket> SocketConnected;

        /// <summary>
        /// 触发 SocketConnected 事件。
        /// </summary>
        /// <param name="client">新连接的客户端 Socket。</param>
        private void OnSocketConnected(Socket client)
        {
            SocketConnected?.Invoke(this, client);
        }
        #endregion

        #region IDisposable 实现
        private Boolean disposed = false;

        ~TcpSocketListener()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Stop();
                    args?.Dispose();
                }

                disposed = true;
            }
        }
        #endregion
    }
}
