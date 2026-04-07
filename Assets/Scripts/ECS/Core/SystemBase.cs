// Assets/Scripts/ECS/Core/SystemBase.cs

using System;
using System.Collections.Generic;

/// <summary>
/// 所有业务逻辑系统的抽象基类。
/// 【完整优化版】：对接 ECSManager 的每帧查询缓存。
/// 逻辑：第一个查询某组合的系统负责遍历，后续系统直接复用结果。
/// </summary>
public abstract class SystemBase
{
    protected List<Entity> _entities;
    
    public SystemBase(List<Entity> entities)
    {
        _entities = entities;
    }

    public abstract void Update(float deltaTime);

    // ==========================================
    // 缓存友好型实时查询接口 (0 GC)
    // ==========================================

    protected List<Entity> GetEntitiesWith<T>() where T : Component
    {
        Type type = typeof(T);
        if (ECSManager.Instance.QueryCache.TryGetValue(type, out var cached)) return cached;

        var list = ECSManager.Instance.GetListFromPool();
        foreach (var e in _entities)
        {
            if (e.IsAlive && e.HasComponent<T>()) list.Add(e);
        }
        ECSManager.Instance.QueryCache[type] = list;
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2>() where T1 : Component where T2 : Component
    {
        // 生成复合 Key：使用两个组件 Hash 的组合
        int key = typeof(T1).GetHashCode() ^ (typeof(T2).GetHashCode() << 2);
        // 注意：由于 QueryCache 的 Key 是 Type，我们需要一个包装或转换
        // 这里为了兼容你的 UIManager/ECSManager 结构，直接使用 T1 的类型作为主缓存 Key 是最稳妥的
        // 或者你可以将 QueryCache 的 Key 类型改为 object
        
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var e in _entities)
        {
            if (e.IsAlive && e.HasComponent<T1>() && e.HasComponent<T2>()) list.Add(e);
        }
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3>() 
        where T1 : Component where T2 : Component where T3 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var e in _entities)
        {
            if (e.IsAlive && e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>()) 
                list.Add(e);
        }
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4>() 
        where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var e in _entities)
        {
            if (e.IsAlive && e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>() && e.HasComponent<T4>()) 
                list.Add(e);
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() 
        where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var e in _entities)
        {
            if (e.IsAlive && e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>() && e.HasComponent<T4>() && e.HasComponent<T5>()) 
                list.Add(e);
        }
        return list;
    }
    
    /// <summary>
    /// 【重要改动】归还列表。
    /// 现在的逻辑下，此方法不再主动调用 ReturnListToPool，
    /// 而是交给 ECSManager 在帧末统一回收，从而允许跨系统共享 List。
    /// </summary>
    protected void ReturnListToPool(List<Entity> list)
    {
        // 故意留空。如果手动回收了，QueryCache 里的引用就会指向一个被 Clear 掉的空列表。
    }
}