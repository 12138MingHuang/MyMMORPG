using System;
using System.Runtime.InteropServices;

/// <summary>
/// 提供计时功能的类，可以用于跟踪游戏或应用程序中的帧时间和经过时间。
/// </summary>
class Time
{
    // 从kernel32.dll中导入QueryPerformanceCounter函数
    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceCounter([In, Out] ref long lpPerformanceCount);

    // 从kernel32.dll中导入QueryPerformanceFrequency函数
    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceFrequency([In, Out] ref long lpFrequency);

    static Time()
    {
        startupTicks = ticks;
    }

    private static long _frameCount = 0;

    /// <summary>
    /// 已经过的帧数（只读）
    /// </summary>
    public static long frameCount { get { return _frameCount; } }

    static long startupTicks = 0;
    static long freq = 0;

    /// <summary>
    /// 获取当前的计时器滴答数
    /// </summary>
    public static long ticks
    {
        get
        {
            long f = freq;

            if (f == 0)
            {
                if (QueryPerformanceFrequency(ref f))
                {
                    freq = f;
                }
                else
                {
                    freq = -1;
                }
            }

            if (f == -1)
            {
                return Environment.TickCount * 10000;
            }

            long c = 0;
            QueryPerformanceCounter(ref c);
            return (long)(((double)c) * 1000 * 10000 / ((double)f));
        }
    }

    private static long lastTick = 0;
    private static float _deltaTime = 0;

    /// <summary>
    /// 完成上一帧所用的时间（秒）（只读）
    /// </summary>
    public static float deltaTime
    {
        get
        {
            return _deltaTime;
        }
    }

    private static float _time = 0;

    /// <summary>
    /// 当前帧开始时的时间（秒）（只读），这是自游戏开始以来的时间
    /// </summary>
    public static float time
    {
        get
        {
            return _time;
        }
    }

    /// <summary>
    /// 自启动以来的真实时间（秒）（只读）
    /// </summary>
    public static float realtimeSinceStartup
    {
        get
        {
            long _ticks = ticks;
            return (_ticks - startupTicks) / 10000000f;
        }
    }

    /// <summary>
    /// 更新计时器，在每帧结束时调用
    /// </summary>
    public static void Tick()
    {
        long _ticks = ticks;

        _frameCount++;
        if (_frameCount == long.MaxValue)
            _frameCount = 0;

        if (lastTick == 0) lastTick = _ticks;
        _deltaTime = (_ticks - lastTick) / 10000000f;
        _time = (_ticks - startupTicks) / 10000000f;
        lastTick = _ticks;
    }
}
