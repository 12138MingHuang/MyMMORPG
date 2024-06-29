using System;

namespace Common.Utils
{
    /// <summary>
    /// 时间工具类，提供时间戳与日期时间之间的转换功能，以及常用的游戏开发时间工具函数。
    /// </summary>
    public class TimeUtil
    {
        /// <summary>
        /// 获取当前时间的时间戳（秒数）。
        /// </summary>
        public static double Timestamp
        {
            get { return GetTimestamp(DateTime.Now); }
        }

        /// <summary>
        /// 将时间戳转换为日期时间。
        /// </summary>
        /// <param name="timeStamp">时间戳，单位为秒。</param>
        /// <returns>转换后的日期时间。</returns>
        public static DateTime GetTime(long timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timeStamp * 10000000; // 时间戳单位是秒，转为100纳秒刻度
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        /// <summary>
        /// 获取指定日期时间的时间戳（秒数）。
        /// </summary>
        /// <param name="time">指定的日期时间。</param>
        /// <returns>时间戳（秒数）。</returns>
        public static double GetTimestamp(DateTime time)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (time - startTime).TotalSeconds;
        }

        /// <summary>
        /// 获取两个日期时间之间的时间间隔（秒数）。
        /// </summary>
        /// <param name="fromTime">起始时间。</param>
        /// <param name="toTime">结束时间。</param>
        /// <returns>时间间隔（秒数）。</returns>
        public static double GetTimeInterval(DateTime fromTime, DateTime toTime)
        {
            return (toTime - fromTime).TotalSeconds;
        }

        /// <summary>
        /// 获取当前帧数。
        /// </summary>
        /// <param name="frameRate">帧率。</param>
        /// <returns>当前帧数。</returns>
        public static int GetCurrentFrame(int frameRate)
        {
            return (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond * frameRate);
        }

        /// <summary>
        /// 检测是否过了每日的午夜（0点）。
        /// </summary>
        /// <param name="time">要检测的时间。</param>
        /// <returns>如果过了午夜返回 true，否则返回 false。</returns>
        public static bool IsPastMidnight(DateTime time)
        {
            TimeSpan midnight = TimeSpan.FromHours(24); // 午夜的时间间隔，24小时制
            return time.TimeOfDay >= midnight;
        }


        /// <summary>
        /// 检测是否过了自定义的跨天时间点。
        /// </summary>
        /// <param name="time">要检测的时间。</param>
        /// <param name="customHour">自定义的小时数，24小时制。</param>
        /// <param name="customMinute">自定义的分钟数。</param>
        /// <param name="customSecond">自定义的秒数。可选参数，默认为0。</param>
        /// <param name="customMillisecond">自定义的毫秒数。可选参数，默认为0。</param>
        /// <returns>如果过了自定义时间点返回 true，否则返回 false。</returns>
        public static bool IsPastCustomDayTime(DateTime time, int customHour, int customMinute, int customSecond = 0, int customMillisecond = 0)
        {
            TimeSpan customTime = new TimeSpan(customHour, customMinute, customSecond, customMillisecond);
            return time.TimeOfDay >= customTime;
        }

        /// <summary>
        /// 检测指定时间是否过了自定义的时间点。
        /// </summary>
        /// <param name="time">要检测的时间。</param>
        /// <param name="customDateTime">自定义的时间点。</param>
        /// <returns>如果过了自定义时间点返回 true，否则返回 false。</returns>
        public static bool IsPastCustomDayTime(DateTime time, DateTime customDateTime)
        {
            return time >= customDateTime;
        }

        /// <summary>
        /// 检测两个时间是否在同一天。
        /// </summary>
        /// <param name="time1">时间1。</param>
        /// <param name="time2">时间2。</param>
        /// <returns>如果在同一天返回 true，否则返回 false。</returns>
        public static bool IsSameDay(DateTime time1, DateTime time2)
        {   
            return time1.Date == time2.Date;
        }

        /// <summary>
        /// 格式化时间为特定的游戏时间显示格式。
        /// </summary>
        /// <param name="time">要格式化的时间。</param>
        /// <param name="format">时间格式字符串，默认是[yyyy-MM-dd HH:mm:ss]。</param>
        /// <returns>格式化后的时间字符串。</returns>
        public static string FormatTime(DateTime time, string format = "yyyy-MM-dd HH:mm:ss")
        {
            return time.ToString(format);
        }

        /// <summary>
        /// 获取指定时间的Unix时间戳（秒数）。Unix时间戳：协调世界时（UTC）1970年1月1日00:00:00以来经过的秒数（不包括闰秒）。
        /// </summary>
        /// <param name="time">指定的时间。</param>
        /// <returns>Unix时间戳（秒数）。</returns>
        public static long GetUnixTimestampSeconds(DateTime time)
        {
            return new DateTimeOffset(time).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 获取指定时间的Unix时间戳（毫秒数）。Unix时间戳：协调世界时（UTC）1970年1月1日00:00:00以来经过的秒数（不包括闰秒）。
        /// </summary>
        /// <param name="time">指定的时间。</param>
        /// <returns>Unix时间戳（毫秒数）。</returns>
        public static long GetUnixTimestampMilliseconds(DateTime time)
        {
            return new DateTimeOffset(time).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 计算两个时间之间的天数差。
        /// </summary>
        /// <param name="time1">时间1。</param>
        /// <param name="time2">时间2。</param>
        /// <returns>天数差。</returns>
        public static int GetDaysDifference(DateTime time1, DateTime time2)
        {
            return (time2.Date - time1.Date).Days;
        }
    }
}
