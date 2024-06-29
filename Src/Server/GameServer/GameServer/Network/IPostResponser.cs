using SkillBridge.Message;

namespace Network
{
    /// <summary>
    /// 定义一个接口，用于在处理完网络消息响应后进行后续处理。
    /// </summary>
    public interface IPostResponser
    {
        /// <summary>
        /// 在处理完网络消息响应后执行后续处理。
        /// </summary>
        /// <param name="message">处理完的网络消息响应。</param>
        void PostProcess(NetMessageResponse message);
    }
}
