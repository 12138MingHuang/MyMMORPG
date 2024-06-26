using log4net;

namespace Common
{
    /// <summary>
    /// 一个使用 log4net 进行日志记录的静态类。
    /// </summary>
    public static class Log
    {
        private static ILog log;

        /// <summary>
        /// 初始化日志记录器。
        /// </summary>
        /// <param name="name">日志记录器的名称。</param>
        public static void Init(string name)
        {
            log = LogManager.GetLogger(name);
        }

        /// <summary>
        /// 记录信息级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Info(object message)
        {
            log.Info(message);
        }

        /// <summary>
        /// 记录格式化的信息级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        public static void InfoFormat(string format, object arg0)
        {
            log.InfoFormat(format, arg0);
        }

        /// <summary>
        /// 记录格式化的信息级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        public static void InfoFormat(string format, object arg0, object arg1)
        {
            log.InfoFormat(format, arg0, arg1);
        }

        /// <summary>
        /// 记录格式化的信息级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        /// <param name="arg2">格式字符串的第三个参数。</param>
        public static void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            log.InfoFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 记录格式化的信息级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void InfoFormat(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        /// <summary>
        /// 记录警告级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Warning(object message)
        {
            log.Warn(message);
        }

        /// <summary>
        /// 记录格式化的警告级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        public static void WarningFormat(string format, object arg0)
        {
            log.WarnFormat(format, arg0);
        }

        /// <summary>
        /// 记录格式化的警告级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        public static void WarningFormat(string format, object arg0, object arg1)
        {
            log.WarnFormat(format, arg0, arg1);
        }

        /// <summary>
        /// 记录格式化的警告级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        /// <param name="arg2">格式字符串的第三个参数。</param>
        public static void WarningFormat(string format, object arg0, object arg1, object arg2)
        {
            log.WarnFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 记录格式化的警告级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void WarningFormat(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        /// <summary>
        /// 记录错误级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Error(object message)
        {
            log.Error(message);
        }

        /// <summary>
        /// 记录格式化的错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        public static void ErrorFormat(string format, object arg0)
        {
            log.ErrorFormat(format, arg0);
        }

        /// <summary>
        /// 记录格式化的错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        public static void ErrorFormat(string format, object arg0, object arg1)
        {
            log.ErrorFormat(format, arg0, arg1);
        }

        /// <summary>
        /// 记录格式化的错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        /// <param name="arg2">格式字符串的第三个参数。</param>
        public static void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            log.ErrorFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 记录格式化的错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void ErrorFormat(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        /// <summary>
        /// 记录严重错误级别的日志消息。
        /// </summary>
        /// <param name="message">要记录的消息对象。</param>
        public static void Fatal(object message)
        {
            log.Fatal(message);
        }

        /// <summary>
        /// 记录格式化的严重错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        public static void FatalFormat(string format, object arg0)
        {
            log.FatalFormat(format, arg0);
        }

        /// <summary>
        /// 记录格式化的严重错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        public static void FatalFormat(string format, object arg0, object arg1)
        {
            log.FatalFormat(format, arg0, arg1);
        }

        /// <summary>
        /// 记录格式化的严重错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="arg0">格式字符串的第一个参数。</param>
        /// <param name="arg1">格式字符串的第二个参数。</param>
        /// <param name="arg2">格式字符串的第三个参数。</param>
        public static void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            log.FatalFormat(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 记录格式化的严重错误级别的日志消息。
        /// </summary>
        /// <param name="format">日志消息的格式字符串。</param>
        /// <param name="args">格式字符串的参数数组。</param>
        public static void FatalFormat(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
    }
}