using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// 网络客户端单例类，继承自MonoSingleton，用于处理网络连接和消息收发
    /// </summary>
    class NetClient : MonoSingleton<NetClient>
    {
        // 默认参数定义
        private const int DEF_POLL_INTERVAL_MILLISECONDS = 100; // 默认网络线程挂起间隔
        private const int DEF_TRY_CONNECT_TIMES = 3;            // 默认重试连接次数
        private const int DEF_RECV_BUFFER_SIZE = 64 * 1024;     // 默认接收缓冲区初始大小
        private const int DEF_PACKAGE_HEADER_LENGTH = 4;        // 默认数据包头部大小
        private const int DEF_SEND_PING_INTERVAL = 30;          // 默认发送ping包间隔
        private const int NetConnectTimeout = 10000;            // 默认连接超时时间（毫秒）
        private const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   // 默认显示加载图标等待时间（毫秒）
        private const int NetReconnectPeriod = 10;              // 默认重新连接周期（秒）

        // 错误码定义
        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;         // 协议错误
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       // 发送异常
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      // 收到非法数据包
        public const int NET_ERROR_ZERO_BYTE = 1002;            // 收发0字节
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      // 收包超时
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        // 代理超时
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      // 连接失败
        public const int NET_ERROR_PROXY_ERROR = 1006;          // 代理重启
        public const int NET_ERROR_ON_DESTROY = 1007;           // 销毁时关闭网络连接
        public const int NET_ERROR_ON_KICKOUT = 25;             // 被踢出

        // 事件定义
        public delegate void ConnectEventHandler(int result, string reason);
        public delegate void ExpectPackageEventHandler();

        public event ConnectEventHandler OnConnect;                // 连接成功事件
        public event ConnectEventHandler OnDisconnect;             // 断开连接事件
        public event ExpectPackageEventHandler OnExpectPackageTimeout;  // 接收数据包超时事件
        public event ExpectPackageEventHandler OnExpectPackageResume;   // 数据包恢复接收事件

        // 成员变量定义
        private IPEndPoint address;             // 连接地址
        private Socket clientSocket;            // Socket 实例
        private MemoryStream sendBuffer = new MemoryStream();   // 发送缓冲区
        private MemoryStream receiveBuffer = new MemoryStream(DEF_RECV_BUFFER_SIZE); // 接收缓冲区
        private Queue<NetMessage> sendQueue = new Queue<NetMessage>(); // 发送消息队列

        private bool connecting = false;        // 连接中标志
        private int retryTimes = 0;             // 当前重试次数
        private int retryTimesTotal = DEF_TRY_CONNECT_TIMES; // 总重试次数
        private float lastSendTime = 0;         // 上次发送时间
        private int sendOffset = 0;             // 发送偏移量

        // 属性定义
        public bool running { get; set; }       // 运行状态
        public PackageHandler packageHandler = new PackageHandler(null); // 数据包处理器实例

        protected override void OnStart()
        {
            running = true;
            MessageDistributer.Instance.ThrowException = true;
        }

        /// <summary>
        /// 触发连接成功事件
        /// </summary>
        protected virtual void RaiseConnected(int result, string reason)
        {
            ConnectEventHandler handler = OnConnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        /// <summary>
        /// 触发断开连接事件
        /// </summary>
        public virtual void RaiseDisonnected(int result, string reason = "")
        {
            ConnectEventHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        /// <summary>
        /// 触发数据包超时事件
        /// </summary>
        protected virtual void RaiseExpectPackageTimeout()
        {
            ExpectPackageEventHandler handler = OnExpectPackageTimeout;
            if (handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// 触发数据包恢复接收事件
        /// </summary>
        protected virtual void RaiseExpectPackageResume()
        {
            ExpectPackageEventHandler handler = OnExpectPackageResume;
            if (handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// 获取当前是否已连接
        /// </summary>
        public bool Connected
        {
            get
            {
                return (clientSocket != default(Socket)) ? clientSocket.Connected : false;
            }
        }

        /// <summary>
        /// 构造函数，初始化网络客户端实例
        /// </summary>
        public NetClient()
        {
        }

        /// <summary>
        /// 重置网络客户端状态，清空缓冲区和事件处理委托
        /// </summary>
        public void Reset()
        {
            MessageDistributer.Instance.Clear();
            sendQueue.Clear();

            sendOffset = 0;
            connecting = false;
            retryTimes = 0;
            lastSendTime = 0;

            OnConnect = null;
            OnDisconnect = null;
            OnExpectPackageTimeout = null;
            OnExpectPackageResume = null;
        }

        /// <summary>
        /// 初始化网络客户端，设置服务器地址和端口
        /// </summary>
        public void Init(string serverIP, int port)
        {
            address = new IPEndPoint(IPAddress.Parse(serverIP), port);
        }

        /// <summary>
        /// 连接服务器，异步连接，连接结果通过事件回调处理
        /// </summary>
        /// <param name="times">重试次数</param>
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)
        {
            if (connecting)
            {
                return;
            }

            if (clientSocket != null)
            {
                clientSocket.Close();
            }

            if (address == default(IPEndPoint))
            {
                throw new Exception("请先调用 Init 方法初始化网络连接地址。");
            }

            connecting = true;
            lastSendTime = 0;

            DoConnect();
        }

        /// <summary>
        /// 销毁网络客户端对象，关闭网络连接
        /// </summary>
        public void OnDestroy()
        {
            Debug.Log("销毁 NetworkManager。");
            CloseConnection(NET_ERROR_ON_DESTROY);
        }

        /// <summary>
        /// 关闭网络连接，根据错误码进行处理
        /// </summary>
        /// <param name="errCode">错误码</param>
        public void CloseConnection(int errCode)
        {
            Debug.LogWarning("关闭连接，错误码: " + errCode.ToString());
            connecting = false;

            if (clientSocket != null)
            {
                clientSocket.Close();
            }

            // 清空缓冲区和发送队列
            MessageDistributer.Instance.Clear();
            sendQueue.Clear();
            receiveBuffer.Position = 0;
            sendBuffer.Position = sendOffset = 0;

            // 根据错误码处理不同情况
            switch (errCode)
            {
                case NET_ERROR_UNKNOW_PROTOCOL:
                    running = false; // 致命错误，停止网络服务
                    break;
                case NET_ERROR_FAIL_TO_CONNECT:
                case NET_ERROR_PROXY_TIMEOUT:
                case NET_ERROR_PROXY_ERROR:
                    // 可以重新连接或其他处理
                    break;
                // 其他离线处理
                case NET_ERROR_ON_KICKOUT:
                case NET_ERROR_ZERO_BYTE:
                case NET_ERROR_ILLEGAL_PACKAGE:
                case NET_ERROR_SEND_EXCEPTION:
                case NET_ERROR_PACKAGE_TIMEOUT:
                default:
                    lastSendTime = 0;
                    RaiseDisonnected(errCode);
                    break;
            }
        }

        /// <summary>
        /// 发送 Protobuf 消息
        /// </summary>
        /// <param name="message">待发送的消息</param>
        public void SendMessage(NetMessage message)
        {
            if (!running)
            {
                return;
            }

            if (!Connected)
            {
                receiveBuffer.Position = 0;
                sendBuffer.Position = sendOffset = 0;

                Connect();
                Debug.Log("在发送消息之前连接服务器！");
                return;
            }

            sendQueue.Enqueue(message);

            if (lastSendTime == 0)
            {
                lastSendTime = Time.time;
            }
        }

        /// <summary>
        /// 执行连接操作
        /// </summary>
        private void DoConnect()
        {
            Debug.Log("NetClient.DoConnect on " + this.address.ToString());
            try
            {
                if (this.clientSocket != null)
                {
                    this.clientSocket.Close();
                }
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Blocking = true;

                Debug.Log(string.Format("第[{0}]次连接服务器 {1}", retryTimes, address) + "\n");

                IAsyncResult result = clientSocket.BeginConnect(address, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout);

                if (success)
                {
                    clientSocket.EndConnect(result);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                }
                Debug.LogErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("DoConnect Exception:" + e.ToString() + "\n");
            }

            if (clientSocket.Connected)
            {
                clientSocket.Blocking = false;
                RaiseConnected(0, "连接成功");
            }
            else
            {
                retryTimes++;
                if (retryTimes >= retryTimesTotal)
                {
                    RaiseConnected(1, "无法连接服务器");
                }
            }

            connecting = false;
        }

        /// <summary>
        /// 保持连接状态，如果未连接则尝试重新连接
        /// </summary>
        /// <returns>是否保持连接</returns>
        private bool KeepConnect()
        {
            if (connecting)
            {
                return false;
            }

            if (address == null)
            {
                return false;
            }

            if (Connected)
            {
                return true;
            }

            if (retryTimes < retryTimesTotal)
            {
                Connect();
            }

            return false;
        }

        /// <summary>
        /// 处理接收数据逻辑
        /// </summary>
        /// <returns>是否处理成功</returns>
        private bool ProcessRecv()
        {
            bool ret = false;
            try
            {
                if (clientSocket.Blocking)
                {
                    Debug.Log("clientSocket.Blocking = true\n");
                }

                bool error = clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Debug.Log("ProcessRecv Poll SelectError\n");
                    CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }

                ret = clientSocket.Poll(0, SelectMode.SelectRead);
                if (ret)
                {
                    int n = clientSocket.Receive(receiveBuffer.GetBuffer(), 0, receiveBuffer.Capacity, SocketFlags.None);
                    if (n <= 0)
                    {
                        CloseConnection(NET_ERROR_ZERO_BYTE);
                        return false;
                    }

                    packageHandler.ReceiveData<NetMessage>(receiveBuffer.GetBuffer(), 0, n);
                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessReceive exception:" + e.ToString() + "\n");
                CloseConnection(NET_ERROR_ILLEGAL_PACKAGE);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理发送数据逻辑
        /// </summary>
        /// <returns>是否处理成功</returns>
        private bool ProcessSend()
        {
            bool ret = false;
            try
            {
                if (clientSocket.Blocking)
                {
                    Debug.Log("clientSocket.Blocking = true\n");
                }

                bool error = clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Debug.Log("ProcessSend Poll SelectError\n");
                    CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }

                ret = clientSocket.Poll(0, SelectMode.SelectWrite);
                if (ret)
                {
                    if (sendBuffer.Position > sendOffset)
                    {
                        int bufsize = (int)(sendBuffer.Position - sendOffset);
                        int n = clientSocket.Send(sendBuffer.GetBuffer(), sendOffset, bufsize, SocketFlags.None);
                        if (n <= 0)
                        {
                            CloseConnection(NET_ERROR_ZERO_BYTE);
                            return false;
                        }

                        sendOffset += n;
                        if (sendOffset >= sendBuffer.Position)
                        {
                            sendOffset = 0;
                            sendBuffer.Position = 0;
                            sendQueue.Dequeue(); // 发送完成后移除消息
                        }
                    }
                    else
                    {
                        if (sendQueue.Count > 0)
                        {
                            NetMessage message = sendQueue.Peek();
                            byte[] package = PackageHandler.PackMessage(message);
                            sendBuffer.Write(package, 0, package.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("ProcessSend exception:" + e.ToString() + "\n");
                CloseConnection(NET_ERROR_SEND_EXCEPTION);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理收到的消息分发逻辑
        /// </summary>
        private void ProceeMessage()
        {
            MessageDistributer.Instance.Distribute();
        }

        /// <summary>
        /// 更新方法，每帧调用以处理网络收发逻辑
        /// </summary>
        public void Update()
        {
            if (!running)
            {
                return;
            }

            if (KeepConnect())
            {
                if (ProcessRecv())
                {
                    if (Connected)
                    {
                        ProcessSend();
                        ProceeMessage();
                    }
                }
            }
        }
    }
}
