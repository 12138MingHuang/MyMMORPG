/// <summary>
/// 通过定义 INCLUDE_CALL_INFO 预处理符号，你可以控制是否包括调用信息。
/// 要启用或禁用调用信息，可以添加或移除 INCLUDE_CALL_INFO 预处理符号。
/// </summary>
//#define INCLUDE_CALL_INFO

using System;
using System.Runtime.CompilerServices;
using log4net;

namespace Common
{
    /// <summary>
    /// 一个使用 log4net 进行日志记录的静态类。
    /// </summary>
    public static class Log
    {
        private static ILog log;
        private static readonly object lockObj = new object();

        /// <summary>
        /// 初始化日志记录器。
        /// </summary>
        /// <param name="name">日志记录器的名称。</param>
        public static void Init(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "Logger name cannot be null or empty.");
            }
            log = LogManager.GetLogger(name);
        }

        /// <summary>
        /// 检查日志记录器是否已初始化。
        /// 如果未初始化，则抛出 InvalidOperationException 异常。
        /// 在每次记录日志之前调用 EnsureLoggerInitialized 方法，检查日志记录器是否已初始化。如果未初始化，则抛出异常
        /// </summary>
        private static void EnsureLoggerInitialized()
        {
            if (log == null)
            {
                throw new InvalidOperationException("Logger is not initialized. Call Init() method first.");
            }
        }

        /// <summary>
        /// 懒加载日志记录器。
        /// 如果日志记录器未初始化，则初始化它。
        /// 在日志记录器未初始化时自动调用 Init 方法进行初始化。
        /// 使用 lock 确保日志记录器的初始化是线程安全的。
        /// </summary>
        private static void LazyLoadLogger()
        {
            if (log == null)
            {
                lock (lockObj)
                {
                    if (log == null)
                    {
                        Init("DefaultLogger");
                    }
                }
            }
        }

        /// <summary>
        /// 记录指定级别的日志消息。
        /// </summary>
        /// <param name="level">日志级别。</param>
        /// <param name="message">要记录的消息对象。</param>
        /// <param name="filePath">调用此方法的文件路径。由编译器自动提供。</param>
        /// <param name="lineNumber">调用此方法的行号。由编译器自动提供。</param>
        /// <param name="memberName">调用此方法的方法名称。由编译器自动提供。</param>
        private static void LogMessage(LogLevel level, object message,
                                       [CallerFilePath] string filePath = "",
                                       [CallerLineNumber] int lineNumber = 0,
                                       [CallerMemberName] string memberName = "")
        {
#if INCLUDE_CALL_INFO
            string logMessage = $"{message} (at {filePath}:{lineNumber} in {memberName})";
#else
            string logMessage = $"{message}";
#endif
            LazyLoadLogger();
            EnsureLoggerInitialized();

            try
            {
                switch (level)
                {
                    case LogLevel.Info:
                        log.Info(logMessage);
                        break;
                    case LogLevel.Warn:
                        log.Warn(logMessage);
                        break;
                    case LogLevel.Error:
                        log.Error(logMessage);
                        break;
                    case LogLevel.Fatal:
                        log.Fatal(logMessage);
                        break;
                    case LogLevel.Debug:
                        log.Debug(logMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                // 处理日志记录中的异常
                Console.Error.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        /// <summary>
        /// 记录指定级别的格式化日志消息。
        /// </summary>
        /// <param name="level">日志级别。</param>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        /// <param name="filePath">调用此方法的文件路径。由编译器自动提供。</param>
        /// <param name="lineNumber">调用此方法的行号。由编译器自动提供。</param>
        /// <param name="memberName">调用此方法的方法名称。由编译器自动提供。</param>
        private static void LogMessageFormat(LogLevel level, string format, object[] args,
                                             [CallerFilePath] string filePath = "",
                                             [CallerLineNumber] int lineNumber = 0,
                                             [CallerMemberName] string memberName = "")
        {
            string logMessage = string.Format(format, args);
            LogMessage(level, logMessage, filePath, lineNumber, memberName);
        }

        /// <summary>
        /// 记录信息级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Info(object message) => LogMessage(LogLevel.Info, message);

        /// <summary>
        /// 记录格式化的信息级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void InfoFormat(string format, params object[] args) => LogMessageFormat(LogLevel.Info, format, args);

        /// <summary>
        /// 记录警告级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Warning(object message) => LogMessage(LogLevel.Warn, message);

        /// <summary>
        /// 记录格式化的警告级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void WarningFormat(string format, params object[] args) => LogMessageFormat(LogLevel.Warn, format, args);

        /// <summary>
        /// 记录错误级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Error(object message) => LogMessage(LogLevel.Error, message);

        /// <summary>
        /// 记录格式化的错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void ErrorFormat(string format, params object[] args) => LogMessageFormat(LogLevel.Error, format, args);

        /// <summary>
        /// 记录严重错误级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Fatal(object message) => LogMessage(LogLevel.Fatal, message);

        /// <summary>
        /// 记录格式化的严重错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void FatalFormat(string format, params object[] args) => LogMessageFormat(LogLevel.Fatal, format, args);

        /// <summary>
        /// 记录调试级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Debug(object message) => LogMessage(LogLevel.Debug, message);

        /// <summary>
        /// 记录格式化的调试级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void DebugFormat(string format, params object[] args) => LogMessageFormat(LogLevel.Debug, format, args);
    }

    /// <summary>
    /// 表示日志级别的枚举类型。
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 信息级别的日志，用于记录一般信息。
        /// </summary>
        Info,

        /// <summary>
        /// 警告级别的日志，用于记录可能导致问题的情况。
        /// </summary>
        Warn,

        /// <summary>
        /// 错误级别的日志，用于记录已发生的错误。
        /// </summary>
        Error,

        /// <summary>
        /// 致命级别的日志，用于记录导致系统崩溃的严重错误。
        /// </summary>
        Fatal,

        /// <summary>
        /// 调试级别的日志，用于记录调试信息。
        /// </summary>
        Debug
    }
}
