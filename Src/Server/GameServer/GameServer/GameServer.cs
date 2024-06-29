using Network;
using System;
using System.Threading;

namespace GameServer
{
    /// <summary>
    /// 表示一个简单的游戏服务器。
    /// </summary>
    class GameServer
    {
        private Thread thread; // 服务器运行线程
        private bool running = false; // 服务器运行状态
        NetService netService;

        /// <summary>
        /// 初始化服务器，创建运行线程。
        /// </summary>
        /// <returns>初始化成功返回 true。</returns>
        public bool Init()
        {
            int Port = Properties.Settings.Default.ServerPort;
            netService = new NetService();
            netService.Init(Port);

            thread = new Thread(new ThreadStart(this.Update)); // 创建新的线程运行Update方法
            return true;
        }

        /// <summary>
        /// 启动服务器，开始运行Update方法。
        /// </summary>
        public void Start()
        {
            running = true; // 设置运行状态为 true
            thread.Start(); // 启动线程
        }

        /// <summary>
        /// 服务器主循环，定期调用 Time.Tick 方法。
        /// </summary>
        private void Update()
        {
            while (running) // 只要服务器处于运行状态
            {
                Time.Tick(); // 调用时间更新方法
                Thread.Sleep(1000); // 休眠1秒
            }
        }

        /// <summary>
        /// 停止服务器，终止运行线程。
        /// </summary>
        public void Stop()
        {
            running = false; // 设置运行状态为 false
            thread.Join(); // 等待线程结束
        }
    }
}
