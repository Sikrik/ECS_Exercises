using System;
using System.Collections.Generic;

/// <summary>
/// 全局事件总线：用于跨模块通信（特别是 ECS 逻辑层与 Unity 表现层之间的解耦）
/// 使用结构体 (struct) 作为事件类型，避免装箱拆箱和 GC 垃圾。
/// </summary>
public static class EventManager
{
    // 存储所有事件的监听者字典
    private static readonly Dictionary<Type, Delegate> _listeners = new Dictionary<Type, Delegate>();

    /// <summary>
    /// 添加监听 (订阅事件)
    /// </summary>
    public static void AddListener<T>(Action<T> listener) where T : struct
    {
        var type = typeof(T);
        if (!_listeners.ContainsKey(type))
        {
            _listeners[type] = null;
        }
        _listeners[type] = (Action<T>)_listeners[type] + listener;
    }

    /// <summary>
    /// 移除监听 (取消订阅)
    /// </summary>
    public static void RemoveListener<T>(Action<T> listener) where T : struct
    {
        var type = typeof(T);
        if (_listeners.ContainsKey(type))
        {
            _listeners[type] = (Action<T>)_listeners[type] - listener;
            if (_listeners[type] == null)
            {
                _listeners.Remove(type);
            }
        }
    }

    /// <summary>
    /// 广播事件 (发布事件)
    /// </summary>
    public static void Broadcast<T>(T eventData) where T : struct
    {
        var type = typeof(T);
        if (_listeners.TryGetValue(type, out var action) && action != null)
        {
            ((Action<T>)action).Invoke(eventData);
        }
    }
}