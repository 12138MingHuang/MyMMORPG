using log4net;
using System;
using UnityEngine;

/// <summary>
/// UnityLogger 类用于将 Unity 的日志输出与 log4net 集成，使得 Unity 的日志可以通过 log4net 进行管理和输出。
/// </summary>
public static class UnityLogger
{
    /// <summary>
    /// 初始化日志系统，将 Unity 的日志事件绑定到自定义处理函数。
    /// </summary>
    public static void Init()
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }

    // 获取名为 "Unity" 的日志记录器实例
    private static ILog log = LogManager.GetLogger("Unity");

    /// <summary>
    /// Unity 日志消息接收处理函数，根据日志类型进行不同级别的日志记录。
    /// </summary>
    /// <param name="condition">日志消息的内容。</param>
    /// <param name="stackTrace">日志消息的堆栈跟踪信息。</param>
    /// <param name="type">日志消息的类型。</param>
    private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            //假设 condition 是 "NullReferenceException"
            //stackTrace 是："at ExampleClass.Method()\nat ExampleClass.Main()"
            //那么这行代码将记录以下内容到错误日志中：
            //NullReferenceException
            //at ExampleClass.Method()
            //at ExampleClass.Main()
            case LogType.Error:
                log.ErrorFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                break;
            case LogType.Assert:
                log.DebugFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                break;
            case LogType.Exception:
                log.FatalFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                break;
            case LogType.Warning:
                log.WarnFormat("{0}\r\n{1}", condition, stackTrace.Replace("\n", "\r\n"));
                break;
            default:
                log.Info(condition);
                break;
        }
    }
}
