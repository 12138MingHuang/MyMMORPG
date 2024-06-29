using System;

namespace Common
{
    /// <summary>
    /// 提供处理字节缓冲区的实用方法，包括从字节数组中获取整数等功能。
    /// </summary>
    public class BufferUtility
    {
        /// <summary>
        /// 从指定的字节数组和偏移量处获取一个 32 位有符号整数。
        /// </summary>
        /// <param name="buffer">包含整数的字节数组</param>
        /// <param name="offset">从字节数组中开始转换的偏移量</param>
        /// <returns>转换后的整数</returns>
        public static int GetInt32(byte[] buffer, int offset)
        {
            return BitConverter.ToInt32(buffer, offset);
        }
    }
}
