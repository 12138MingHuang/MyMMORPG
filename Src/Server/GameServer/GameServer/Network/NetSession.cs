using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;

namespace Network
{
    /// <summary>
    /// 实现 INetSession 接口的具体网络会话类。
    /// </summary>
    class NetSession : INetSession
    {
        ///// <summary>
        ///// 会话关联的用户。
        ///// </summary>
        //public TUser User { get; set; }

        ///// <summary>
        ///// 会话关联的角色。
        ///// </summary>
        //public Character Character { get; set; }

        ///// <summary>
        ///// 会话关联的实体。
        ///// </summary>
        //public NEntity Entity { get; set; }

        /// <summary>
        /// 在断开连接时处理角色离开。
        /// </summary>
        public void Disconnected()
        {
            //if (Character != null)
            //    UserService.Instance.CharacterLeave(Character);
        }

        private NetMessage message;

        /// <summary>
        /// 获取会话的响应消息。如果消息或消息响应为空，则初始化它们。
        /// </summary>
        public NetMessageResponse Response
        {
            get
            {
                if (this.message == null)
                    this.message = new NetMessage();
                if (this.message.Response == null)
                    this.message.Response = new NetMessageResponse();
                return this.message.Response;
            }
        }

        /// <summary>
        /// 获取会话的响应数据并进行打包。
        /// </summary>
        /// <returns>打包后的响应数据，如果没有响应数据则返回 null。</returns>
        public byte[] GetResponse()
        {
            if (this.message != null)
            {
                //if (Character != null && Character.StatusManager.HasStatus)
                //{
                //    Character.StatusManager.ApplyResponse(Response);
                //}

                byte[] data = PackageHandler.PackMessage(this.message);
                this.message = null; // 清空消息，准备下一次的响应
                return data;
            }
            return null;
        }
    }
}
