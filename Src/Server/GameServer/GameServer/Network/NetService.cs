using System;
using System.Net;
using System.Net.Sockets;
using GameServer;
using Common;

namespace Network
{
    /// <summary>
    /// 网络服务类，负责管理服务器端的网络连接。
    /// </summary>
    class NetService
    {
        /// <summary>
        /// TCP套接字监听器实例，用于监听和处理传入的连接。
        /// </summary>
        static TcpSocketListener ServerListener;

        /// <summary>
        /// 初始化网络服务。
        /// </summary>
        /// <param name="port">监听端口。</param>
        /// <returns>初始化成功返回 true。</returns>
        public bool Init(int port)
        {
            ServerListener = new TcpSocketListener("127.0.0.1", GameServer.Properties.Settings.Default.ServerPort, 10);
            ServerListener.SocketConnected += OnSocketConnected;
            return true;
        }

        /// <summary>
        /// 启动网络服务。
        /// </summary>
        public void Start()
        {
            // 启动监听
            Log.Warning("启动监听...");
            ServerListener.Start();

            Log.Warning("开始启动网络服务...");

            // 启动消息分发器
            MessageDistributer<NetConnection<NetSession>>.Instance.Start(8);
            Log.Warning("网络服务启动成功");
        }

        /// <summary>
        /// 停止网络服务。
        /// </summary>
        public void Stop()
        {
            Log.Warning("停止监听...");
            Log.Warning("停止网络服务...");

            // 停止监听
            ServerListener.Stop();

            // 停止消息处理器
            MessageDistributer<NetConnection<NetSession>>.Instance.Stop();
            Log.Warning("网络服务停止成功");
        }

        /// <summary>
        /// 处理新连接的回调函数。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">包含连接信息的 Socket。</param>
        private void OnSocketConnected(object sender, Socket e)
        {
            IPEndPoint clientIP = (IPEndPoint)e.RemoteEndPoint;
            // 可以在这里对IP做一级验证，比如黑名单

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            NetSession session = new NetSession();

            NetConnection<NetSession> connection = new NetConnection<NetSession>(e, args,
                new NetConnection<NetSession>.DataReceivedCallback(DataReceived),
                new NetConnection<NetSession>.DisconnectedCallback(Disconnected), session);

            Log.WarningFormat("客户端[{0}] 已连接", clientIP);
        }

        /// <summary>
        /// 连接断开回调函数。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">包含断开连接信息的 SocketAsyncEventArgs。</param>
        static void Disconnected(NetConnection<NetSession> sender, SocketAsyncEventArgs e)
        {
            sender.Session.Disconnected();
            Log.WarningFormat("客户端[{0}] 已断开连接", e.RemoteEndPoint);
        }

        /// <summary>
        /// 接收数据回调函数。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">包含接收数据的信息。</param>
        static void DataReceived(NetConnection<NetSession> sender, DataEventArgs e)
        {
            Log.WarningFormat("客户端[{0}] 接收数据，长度:{1}", e.RemoteEndPoint, e.Length);

            // 由包处理器处理封包
            lock (sender.packageHandler)
            {
                sender.packageHandler.ReceiveData(e.Data, 0, e.Data.Length);
            }
        }
    }
}
