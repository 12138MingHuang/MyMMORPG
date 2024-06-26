using System;
using System.IO;
using Common;
using GameServer;
using log4net.Config;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            // 定义log4net配置文件的路径
            string configFilePath = @"D:\MyUnityProject\MyMMORPG\Src\Server\GameServer\GameServer\log4net.xml";
            FileInfo fi = new FileInfo(configFilePath);

            // 检查log4net配置文件是否存在
            if (!fi.Exists)
            {
                Console.WriteLine("log4net.xml 配置文件不存在: " + configFilePath);
                return;
            }

            // 配置并监视log4net配置文件
            XmlConfigurator.ConfigureAndWatch(fi);
            Log.Init("GameServer");
            Log.Info("Game Server Init");

            // 定义日志目录路径
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            // 检查并创建日志目录
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            // 初始化并启动GameServer
            //GameServer server = new GameServer();
            //server.Init();
            //server.Start();
            Console.WriteLine("Game Server Running......");

            // 运行命令行助手
            CommandHelper.Run();

            // 记录GameServer退出日志并停止服务器
            Log.Info("Game Server Exiting...");
            //server.Stop();
            Log.Info("Game Server Exited");
        }
        catch (Exception ex)
        {
            // 捕获并记录异常
            Console.WriteLine("异常: " + ex.Message);
            Log.Error("异常: " + ex.Message);
        }
    }
}