using System.Collections.Generic;

/// <summary>
/// 所有业务逻辑系统的抽象基类。
/// 【优化版】：移除了危险的 QueryCache 缓存，采用实时查询，彻底杜绝数据滞后 Bug。
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
    // 实时查询接口 (从 ECSManager 的列表池中借用 List 以做到 0 GC)
    // ==========================================

    protected List<Entity> GetEntitiesWith<T>() where T : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T>()) list.Add(entity);
        }
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2>() where T1 : Component where T2 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>()) list.Add(entity);
        }
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3>() where T1 : Component where T2 : Component where T3 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>()) list.Add(entity);
        }
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>() && entity.HasComponent<T4>()) list.Add(entity);
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
    {
        var list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>() && entity.HasComponent<T4>() && entity.HasComponent<T5>()) list.Add(entity);
        }
        return list;
    }
    
    protected void ReturnListToPool(List<Entity> list)
    {
        ECSManager.Instance.ReturnListToPool(list);
    }
}