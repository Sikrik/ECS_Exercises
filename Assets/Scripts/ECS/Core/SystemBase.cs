// Assets/Scripts/ECS/Core/SystemBase.cs
using System;
using System.Collections.Generic;

/// <summary>
/// 所有业务逻辑系统的抽象基类。
/// 优化版：修正了多组件查询的冗余缓存计算，单组件查询完美复用，实现 0 GC。
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
}