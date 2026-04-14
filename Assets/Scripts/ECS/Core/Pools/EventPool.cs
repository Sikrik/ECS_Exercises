// 路径: Assets/Scripts/ECS/Core/Pools/EventPool.cs
using System.Collections.Generic;

// 1. 定义事件清理接口
public interface IPooledEvent
{
    // 回收前调用，用于清空自身数据（如引用类型设为 null，防止内存泄漏）
    void Clear();
}

// 2. 通用泛型事件对象池
// 约束 T 必须是 Component，必须实现 IPooledEvent，且必须有无参构造函数 new()
public static class EventPool<T> where T : Component, IPooledEvent, new()
{
    private static readonly Stack<T> _pool = new Stack<T>();

    public static T Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : new T();
    }

    public static void Return(T evt)
    {
        evt.Clear(); // 强制洗干净再放入池子
        _pool.Push(evt);
    }
}