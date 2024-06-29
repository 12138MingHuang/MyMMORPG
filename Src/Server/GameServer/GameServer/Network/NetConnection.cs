using Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    /// <summary>
    /// 表示与服务器的连接。
    /// </summary>
    public class NetConnection<T> where T : INetSession
    {
        /// <summary>
        /// 数据接收回调，用于通知监听器 ServerConnection 接收到数据。
        /// </summary>
        /// <param name="sender">回调的发送者。</param>
        /// <param name="e">包含接收数据的 DataEventArgs 对象。</param>
        public delegate void DataReceivedCallback(NetConnection<T> sender, DataEventArgs e);

        /// <summary>
        /// 断开连接回调，用于通知监听器 ServerConnection 已断开连接。
        /// </summary>
        /// <param name="sender">回调的发送者。</param>
        /// <param name="e">ServerConnection 使用的 SocketAsyncEventArgs 对象。</param>
        public delegate void DisconnectedCallback(NetConnection<T> sender, SocketAsyncEventArgs e);

        #region 内部类

        /// <summary>
        /// 内部状态类，用于存储连接的相关信息和回调方法。
        /// </summary>
        internal class State
        {
            /// <summary>
            /// 数据接收回调委托。
            /// </summary>
            public DataReceivedCallback dataReceived;

            /// <summary>
            /// 断开连接回调委托。
            /// </summary>
            public DisconnectedCallback disconnectedCallback;

            /// <summary>
            /// 当前连接的套接字。
            /// </summary>
            public Socket socket;
        }

        #endregion

        #region 字段

        /// <summary>
        /// 异步套接字事件参数，用于处理异步接收和发送操作。
        /// </summary>
        private SocketAsyncEventArgs eventArgs;

        /// <summary>
        /// 封包处理器，用于处理与连接相关的数据包。
        /// </summary>
        public PackageHandler<NetConnection<T>> packageHandler;

        #endregion


        #region 构造函数

        /// <summary>
        /// 与服务器的连接，总是异步监听。
        /// </summary>
        /// <param name="socket">连接的 Socket。</param>
        /// <param name="args">用于异步接收的 SocketAsyncEventArgs。</param>
        /// <param name="dataReceived">接收到数据时调用的回调。</param>
        /// <param name="disconnectedCallback">断开连接时调用的回调。</param>
        /// <param name="session">会话对象。</param>
        public NetConnection(Socket socket, SocketAsyncEventArgs args, DataReceivedCallback dataReceived,
            DisconnectedCallback disconnectedCallback, T session)
        {
            lock (this)
            {
                // 初始化封包处理器
                this.packageHandler = new PackageHandler<NetConnection<T>>(this);

                // 创建并初始化内部状态对象
                State state = new State()
                {
                    socket = socket,
                    dataReceived = dataReceived,
                    disconnectedCallback = disconnectedCallback
                };

                // 初始化 SocketAsyncEventArgs
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.AcceptSocket = socket; // 设置接受套接字
                eventArgs.Completed += ReceivedCompleted; // 注册完成事件处理器
                eventArgs.UserToken = state; // 将状态对象保存到 UserToken
                eventArgs.SetBuffer(new byte[64 * 1024], 0, 64 * 1024); // 设置缓冲区大小

                // 开始异步接收数据
                BeginReceive(eventArgs);

                // 设置会话对象
                this.session = session;
            }
        }

        #endregion


        #region 公共方法
        /// <summary>
        /// 断开客户端连接。
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                CloseConnection(eventArgs);
            }
        }

        /// <summary>
        /// 向客户端发送数据。
        /// </summary>
        /// <param name="data">要发送的数据。</param>
        /// <param name="offset">数据的偏移量。</param>
        /// <param name="count">要发送的数据量。</param>
        public void SendData(Byte[] data, Int32 offset, Int32 count)
        {
            lock (this)
            {
                State state = eventArgs.UserToken as State;
                Socket socket = state.socket;
                if (socket.Connected)
                    socket.BeginSend(data, 0, count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
        }

        /// <summary>
        /// 发送响应数据。
        /// </summary>
        public void SendResponse()
        {
            byte[] data = session.GetResponse();
            this.SendData(data, 0, data.Length);
        }

        /// <summary>
        /// 发送数据回调。
        /// </summary>
        /// <param name="ar">异步操作状态。</param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // 从状态对象中检索 socket。
                Socket client = (Socket)ar.AsyncState;

                // 完成向远程设备发送数据。
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Log.Info(e.ToString());
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 开始异步接收数据。
        /// </summary>
        /// <param name="args">用于操作的 SocketAsyncEventArgs。</param>
        private void BeginReceive(SocketAsyncEventArgs args)
        {
            lock (this)
            {
                Socket socket = (args.UserToken as State).socket;
                if (socket.Connected)
                {
                    args.AcceptSocket.ReceiveAsync(args);
                }
            }
        }

        /// <summary>
        /// 当异步接收操作完成时调用。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="args">操作的 SocketAsyncEventArgs。</param>
        private void ReceivedCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred == 0)
            {
                CloseConnection(args); // 正常断开连接
                return;
            }
            if (args.SocketError != SocketError.Success)
            {
                CloseConnection(args); // 非正常断开连接
                return;
            }

            State state = args.UserToken as State;

            Byte[] data = new Byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, data, 0, data.Length);
            OnDataReceived(data, args.RemoteEndPoint as IPEndPoint, state.dataReceived);

            BeginReceive(args);
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        /// <param name="args">连接的 SocketAsyncEventArgs。</param>
        private void CloseConnection(SocketAsyncEventArgs args)
        {
            State state = args.UserToken as State;
            Socket socket = state.socket;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { } // 如果客户端进程已经关闭，则会抛出异常
            socket.Close();
            socket = null;

            args.Completed -= ReceivedCompleted; // 必须记住这一点！
            OnDisconnected(args, state.disconnectedCallback);
        }
        #endregion

        #region 事件
        /// <summary>
        /// 触发 DataReceivedCallback。
        /// </summary>
        /// <param name="data">接收到的数据。</param>
        /// <param name="remoteEndPoint">数据来源的地址。</param>
        /// <param name="callback">回调。</param>
        private void OnDataReceived(Byte[] data, IPEndPoint remoteEndPoint, DataReceivedCallback callback)
        {
            callback(this, new DataEventArgs() { RemoteEndPoint = remoteEndPoint, Data = data, Offset = 0, Length = data.Length });
        }

        /// <summary>
        /// 触发 DisconnectedCallback。
        /// </summary>
        /// <param name="args">此连接的 SocketAsyncEventArgs。</param>
        /// <param name="callback">回调。</param>
        private void OnDisconnected(SocketAsyncEventArgs args, DisconnectedCallback callback)
        {
            callback(this, args);
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置连接的认证状态。
        /// true: 已认证
        /// false: 未认证
        /// </summary>
        public bool Verified { get; set; }

        private T session;
        /// <summary>
        /// 获取或设置一个会话对象。
        /// </summary>
        public T Session { get { return session; } }
        #endregion
    }
}
