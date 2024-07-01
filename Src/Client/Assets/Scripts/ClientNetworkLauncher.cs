using Network;
using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientNetworkLauncher : MonoBehaviour
{
    private void Start()
    {
        NetClient.Instance.Init("127.0.0.1", 8000);
        NetClient.Instance.Connect();

        NetMessage msg = new NetMessage();
        msg.Request = new NetMessageRequest();
        msg.Response = new NetMessageResponse();

        // 测试客户端发消息
        msg.Request.MyFirstRequest = new FirstTestRequest();
        msg.Request.MyFirstRequest.MyFirstRequestVar = "hello firstRequest";
        NetClient.Instance.SendMessage(msg);
    }

    private void Update()
    {
        
    }
}
