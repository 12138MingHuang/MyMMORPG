using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Google.Protobuf;

namespace Network
{
    /// <summary>
    /// 非泛型的PackageHandler，用于处理数据包。默认消息发送者类型为object。
    /// </summary>
    public class PackageHandler : PackageHandler<object>
    {
        /// <summary>
        /// 使用指定的消息发送者创建PackageHandler实例。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        public PackageHandler(object sender) : base(sender)
        {
        }
    }

    /// <summary>
    /// 泛型的PackageHandler，用于处理数据包。
    /// </summary>
    /// <typeparam name="T">消息发送者类型</typeparam>
    public class PackageHandler<T>
    {
        /// <summary>
        /// 内部内存流，用于存储接收的数据。
        /// </summary>
        private MemoryStream stream = new MemoryStream(64 * 1024);

        /// <summary>
        /// 当前读取位置的偏移量。
        /// </summary>
        private int readOffset = 0;

        /// <summary>
        /// 消息发送者。
        /// </summary>
        private T sender;

        /// <summary>
        /// 使用指定的消息发送者创建PackageHandler实例。
        /// </summary>
        /// <param name="sender">消息发送者</param>
        public PackageHandler(T sender)
        {
            this.sender = sender;
        }

        /// <summary>
        /// 接收数据到 PackageHandler。
        /// </summary>
        /// <typeparam name="Tm">消息的类型，必须实现 Google.Protobuf.IMessage 接口</typeparam>
        /// <param name="data">接收的数据</param>
        /// <param name="offset">数据偏移量</param>
        /// <param name="count">数据长度</param>
        /// <exception cref="Exception">如果写入缓冲区溢出或解析数据包失败，抛出异常</exception>
        public void ReceiveData<Tm>(byte[] data, int offset, int count) where Tm : class, IMessage<Tm>, new()
        {
            // 检查写入缓冲区是否会溢出
            if (stream.Position + count > stream.Capacity)
            {
                throw new Exception("PackageHandler write buffer overflow");
            }
            // 写入数据到内存流
            stream.Write(data, offset, count);

            // 解析数据包
            bool result = ParsePackage<Tm>();
            if (!result)
            {
                throw new Exception("Failed to parse package.");
            }
        }


        /// <summary>
        /// 打包消息。
        /// </summary>
        /// <typeparam name="TMessage">消息的类型，必须实现 Google.Protobuf.IMessage 接口</typeparam>
        /// <param name="message">要打包的消息</param>
        /// <returns>打包后的字节数组，包含消息长度和消息内容</returns>
        public static byte[] PackMessage<TMessage>(TMessage message) where TMessage : class, IMessage
        {
            byte[] package = null;
            using (MemoryStream ms = new MemoryStream())
            {
                // 使用Google.Protobuf序列化消息
                message.WriteTo(ms);

                // 创建包含消息长度和内容的字节数组
                package = new byte[ms.Length + 4];

                // 将消息长度写入前四个字节
                Buffer.BlockCopy(BitConverter.GetBytes((int)ms.Length), 0, package, 0, 4);

                // 将消息内容写入后续字节
                Buffer.BlockCopy(ms.GetBuffer(), 0, package, 4, (int)ms.Length);
            }
            return package;
        }

        /// <summary>
        /// 解包消息。
        /// </summary>
        /// <typeparam name="Tm">消息的类型，必须实现 Google.Protobuf.IMessage 接口</typeparam>
        /// <param name="packet">接收到的数据包</param>
        /// <param name="offset">数据偏移量</param>
        /// <param name="length">数据长度</param>
        /// <returns>解包后的消息</returns>
        public static Tm UnpackMessage<Tm>(byte[] packet, int offset, int length) where Tm : class, IMessage<Tm>, new()
        {
            Tm message = new Tm();
            using (MemoryStream ms = new MemoryStream(packet, offset, length))
            {
                // 使用Google.Protobuf反序列化消息
                message.MergeFrom(ms);
            }
            return message;
        }

        /// <summary>
        /// 解析数据包。
        /// </summary>
        /// <typeparam name="Tm">消息的类型，必须实现 Google.Protobuf.IMessage 接口</typeparam>
        /// <returns>解析成功返回true</returns>
        private bool ParsePackage<Tm>() where Tm : class, IMessage<Tm>, new()
        {
            // 检查是否有完整的数据包（包头大小是4字节）
            if (readOffset + 4 < stream.Position)
            {
                int packageSize = BitConverter.ToInt32(stream.GetBuffer(), readOffset);
                // 检查数据包是否完整
                if (packageSize + readOffset + 4 <= stream.Position)
                {
                    // 解析数据包
                    Tm message = UnpackMessage<Tm>(stream.GetBuffer(), this.readOffset + 4, packageSize);
                    if (message == null)
                    {
                        throw new Exception("PackageHandler ParsePackage failed, invalid package");
                    }
                    // 接收消息
                    MessageDistributer<T>.Instance.ReceiveMessage(this.sender, message);
                    // 更新读取偏移量
                    this.readOffset += (packageSize + 4);
                    // 递归解析下一个包
                    return ParsePackage<Tm>();
                }
            }

            // 未接收完/要结束了
            if (this.readOffset > 0)
            {
                long size = stream.Position - this.readOffset;
                if (this.readOffset < stream.Position)
                {
                    // 将剩余数据移到流的开始位置
                    Array.Copy(stream.GetBuffer(), this.readOffset, stream.GetBuffer(), 0, stream.Position - this.readOffset);
                }
                // 重置流
                this.readOffset = 0;
                stream.Position = size;
                stream.SetLength(size);
            }
            return true;
        }
    }
}

