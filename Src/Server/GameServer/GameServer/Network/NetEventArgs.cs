using System;
using System.Net;

namespace Network
{
    /// <summary>
    /// 包含 Byte[] 的 EventArgs 类。
    /// </summary>
    public class DataEventArgs : EventArgs
    {
        /// <summary>
        /// 远程端点的网络终结点。
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// 包含数据的字节数组。
        /// </summary>
        public Byte[] Data { get; set; }

        /// <summary>
        /// 数据在字节数组中的起始偏移量。
        /// </summary>
        public Int32 Offset { get; set; }

        /// <summary>
        /// 数据的长度。
        /// </summary>
        public Int32 Length { get; set; }
    }
}
