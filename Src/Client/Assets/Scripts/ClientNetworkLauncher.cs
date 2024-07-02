using log4net;
using Network;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ClientNetworkLauncher : MonoBehaviour
{
    private void Awake()
    {
        // 获取 log4net.xml 文件的正确路径
        string projectRootPath = Application.dataPath;  // 获取项目根目录
        string log4netConfigPath = Path.Combine(projectRootPath, "log4net.xml").Replace("\\", "/");  // 构建 log4net.xml 文件的完整路径

        // 检查并创建日志目录
        string logDirectory = Path.Combine(projectRootPath.Replace("/Assets", ""), "Log").Replace("\\", "/");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // 配置 log4net
        log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netConfigPath));

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
