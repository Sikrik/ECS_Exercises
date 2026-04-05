using System.Collections.Generic;

/// <summary>
/// ECS架构中所有系统的抽象基类
/// </summary>
public abstract class SystemBase
{
    protected List<Entity> _entities;
    
    // 缓存Key的占位结构体
    private struct CacheKey<T1> {}
    private struct CacheKey<T1, T2> {}
    private struct CacheKey<T1, T2, T3> {}
    private struct CacheKey<T1, T2, T3, T4> {}
    
    public SystemBase(List<Entity> entities)
    {
        _entities = entities;
    }

    public abstract void Update(float deltaTime);
    
    protected List<Entity> GetEntitiesWith<T>() where T : Component
    {
        var key = typeof(CacheKey<T>);
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T>())
            {
                list.Add(entity);
            }
        }
        
        ECSManager.Instance.QueryCache[key] = list;
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2>() where T1 : Component where T2 : Component
    {
        var key = typeof(CacheKey<T1, T2>);
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>())
            {
                list.Add(entity);
            }
        }
        
        ECSManager.Instance.QueryCache[key] = list;
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3>() where T1 : Component where T2 : Component where T3 : Component
    {
        var key = typeof(CacheKey<T1, T2, T3>);
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>())
            {
                list.Add(entity);
            }
        }
        
        ECSManager.Instance.QueryCache[key] = list;
        return list;
    }
    
    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        var key = typeof(CacheKey<T1, T2, T3, T4>);
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>() && entity.HasComponent<T4>())
            {
                list.Add(entity);
            }
        }
        
        ECSManager.Instance.QueryCache[key] = list;
        return list;
    }
    
    protected void ReturnListToPool(List<Entity> list)
    {
        ECSManager.Instance.ReturnListToPool(list);
    }
}