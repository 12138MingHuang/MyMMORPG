using Common;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Network;
using SkillBridge.Message;
using System.IO;
using System.Linq;
using UnityEngine;

public class ClientNetworkLauncher : MonoBehaviour
{
    private void Awake()
    {
        // 获取 log4net.xml 文件的正确路径
        string projectRootPath = Application.dataPath.Replace("/", "\\");  // 获取项目根目录
        string log4netConfigPath = Path.Combine(projectRootPath, "log4net.xml");  // 构建 log4net.xml 文件的完整路径
        
        FileInfo fi = new FileInfo(log4netConfigPath);

        // 检查log4net配置文件是否存在
        if (!fi.Exists)
        {
            Log.Info("log4net.xml 配置文件不存在: " + log4netConfigPath);
            return;
        }

        // 配置并监视log4net配置文件
        XmlConfigurator.ConfigureAndWatch(fi);

        // 检查并创建日志目录
        string logDirectory = Path.Combine(projectRootPath.Replace("Assets", ""), "Log");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // 获取当前配置的 repository
        Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
        RollingFileAppender rollingFileAppender = (RollingFileAppender)hierarchy.GetAppenders().FirstOrDefault(appender => appender.Name == "UnityLog");
        if (rollingFileAppender != null)
        {
            // 动态设置日志文件路径为目录
            rollingFileAppender.File = logDirectory + "\\";
            rollingFileAppender.ActivateOptions(); // 激活选项
        }

        UnityLogger.Init();
    }

    private void Start()
    {
        NetClient.Instance.Init("127.0.0.1", 8000);
        NetClient.Instance.Connect();

        NetMessage msg = new NetMessage();
        msg.Request = new NetMessageRequest();

        // 测试客户端发消息
        msg.Request.MyFirstRequest = new FirstTestRequest();
        msg.Request.MyFirstRequest.MyFirstRequestVar = "hello firstRequest";
        NetClient.Instance.SendMessage(msg);
    }

    private void Update()
    {
        
    }
}
