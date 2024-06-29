using System;

namespace GameServer
{
    /// <summary>
    /// 提供命令行交互帮助的类。
    /// </summary>
    class CommandHelper
    {
        /// <summary>
        /// 运行命令行循环，处理用户输入的命令。
        /// </summary>
        public static void Run()
        {
            bool run = true; // 表示命令行循环是否继续运行
            while (run)
            {
                Console.Write("> 游戏服务器正在启动中，你可以输入命令来进一步操作游戏服务器 (exit/help): "); // 提示符
                string line = Console.ReadLine(); // 读取用户输入
                switch (line.ToLower().Trim()) // 转换输入为小写并去除两端空白
                {
                    case "exit":
                        run = false; // 如果输入为 "exit"，退出循环
                        break;
                    default:
                        Help(); // 显示帮助信息
                        break;
                }
            }
        }

        /// <summary>
        /// 显示帮助信息，列出可用的命令。
        /// </summary>
        public static void Help()
        {
            Console.Write(@"
Help:
    exit    Exit Game Server
    help    Show Help
");
        }
    }
}
