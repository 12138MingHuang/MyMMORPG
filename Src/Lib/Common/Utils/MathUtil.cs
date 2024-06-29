using System;

namespace Common.Utils
{
    /// <summary>
    /// 提供常用的数学计算工具
    /// </summary>
    public class MathUtil
    {
        /// <summary>
        /// 将浮点数四舍五入到最接近的整数。
        /// </summary>
        /// <param name="f">要四舍五入的浮点数。</param>
        /// <returns>最接近的整数。</returns>
        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }

        /// <summary>
        /// 将角度转换为弧度。
        /// </summary>
        /// <param name="degrees">角度值。</param>
        /// <returns>对应的弧度值。</returns>
        public static float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180f;
        }

        /// <summary>
        /// 将弧度转换为角度。
        /// </summary>
        /// <param name="radians">弧度值。</param>
        /// <returns>对应的角度值。</returns>
        public static float RadiansToDegrees(float radians)
        {
            return radians * 180f / (float)Math.PI;
        }

        /// <summary>
        /// 计算两点之间的距离的平方，避免了开方运算。
        /// </summary>
        /// <param name="x1">第一个点的X坐标。</param>
        /// <param name="y1">第一个点的Y坐标。</param>
        /// <param name="x2">第二个点的X坐标。</param>
        /// <param name="y2">第二个点的Y坐标。</param>
        /// <returns>两点之间距离的平方。</returns>
        public static float DistanceSquared(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// 计算三维空间中两点之间的距离的平方，避免了开方运算。
        /// </summary>
        /// <param name="x1">第一个点的X坐标。</param>
        /// <param name="y1">第一个点的Y坐标。</param>
        /// <param name="z1">第一个点的Z坐标。</param>
        /// <param name="x2">第二个点的X坐标。</param>
        /// <param name="y2">第二个点的Y坐标。</param>
        /// <param name="z2">第二个点的Z坐标。</param>
        /// <returns>两点之间距离的平方。</returns>
        public static float DistanceSquared(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float dz = z2 - z1;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// 限制值在指定的范围内。
        /// </summary>
        /// <param name="value">要限制的值。</param>
        /// <param name="min">最小值。</param>
        /// <param name="max">最大值。</param>
        /// <returns>在指定范围内的值。</returns>
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 将值限制在0到1之间。
        /// </summary>
        /// <param name="value">要限制的值。</param>
        /// <returns>在0到1之间的值。</returns>
        public static float Clamp0_1(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }

        /// <summary>
        /// 线性插值。
        /// </summary>
        /// <param name="a">起始值。</param>
        /// <param name="b">目标值。</param>
        /// <param name="t">插值因子，通常在0到1之间。</param>
        /// <returns>插值结果。</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp0_1(t);
        }
    }
}
