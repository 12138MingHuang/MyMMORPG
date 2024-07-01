using Common;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Servers
{
    internal class FirstServer : Singleton<FirstServer>
    {
        public FirstServer() 
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.FirstTestRequest);
        }

        public void Init()
        {
            Log.Info("FirstServer初始化");
        }

        private void FirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest message)
        {
            sender.Session.Response.MyFirstResponse = new FirstTestResponse();

            Log.InfoFormat("客户端发来的消息是：{0}", sender.Session.Response.MyFirstResponse.MyFirstResponseVar);
        }
    }
}
