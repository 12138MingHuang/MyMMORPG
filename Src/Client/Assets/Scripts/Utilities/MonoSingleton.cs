using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoSingleton 类提供一个抽象的单例模式实现，继承自 MonoBehaviour。
/// 该类确保类型 T 只有一个实例，并提供一个全局访问点。
/// </summary>
/// <typeparam name="T">希望实现单例模式的 MonoBehaviour 类型。</typeparam>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// 指示该单例是否在场景切换时保持不被销毁。
    /// </summary>
    public bool global = true;

    // 静态实例变量
    private static T instance;

    /// <summary>
    /// 获取类型 T 的单例实例。如果实例不存在，则查找场景中的实例。
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();
            }
            return instance;
        }
    }

    /// <summary>
    /// MonoBehaviour 的 Awake 方法，在实例化时调用。
    /// </summary>
    private void Awake()
    {
        Debug.LogWarningFormat("{0}[{1}] Awake", typeof(T), this.GetInstanceID());
        if (global)
        {
            if (instance != null && instance != this.gameObject.GetComponent<T>())
            {
                Destroy(instance);
            }
            DontDestroyOnLoad(this.gameObject);
            instance = this.gameObject.GetComponent<T>();
        }
        this.OnStart();
    }

    /// <summary>
    /// 可被子类重写的启动方法，Awake 方法中调用。
    /// </summary>
    protected virtual void OnStart()
    {

    }
}
