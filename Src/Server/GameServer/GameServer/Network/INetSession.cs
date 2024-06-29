using SkillBridge.Message;
using System;

namespace Network
{
    /// <summary>
    /// 定义网络会话接口。
    /// </summary>
    public interface INetSession
    {
        /// <summary>
        /// 获取会话的响应数据。
        /// </summary>
        /// <returns>会话的响应数据，以字节数组形式返回。</returns>
        byte[] GetResponse();
    }
}
