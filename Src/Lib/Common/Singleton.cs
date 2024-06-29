using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 泛型单例类，确保类型T只有一个实例。
    /// </summary>
    /// <typeparam name="T">必须有无参构造函数的类型。</typeparam>
    public class Singleton<T> where T : new() //类型参数T必须有一个无参构造函数
    {
        /// <summary>
        /// 存储单例实例的私有静态字段。
        /// </summary>
        private static T instance;

        /// <summary>
        /// 获取单例实例的公共静态属性。
        /// </summary>
        public static T Instance
        {
            get
            {
                // 如果instance是默认值（null），则创建一个新的实例，否则返回现有实例
                // default(T) 对于引用类型（如 class），default(T) 返回 null。
                // 对于值类型（如 int、float、struct），default(T) 返回该类型的默认值。例如，int 的默认值是 0，bool 的默认值是 false。
                return Equals(instance, default(T)) ? (instance = new T()) : instance;
            }
        }
    }
}

