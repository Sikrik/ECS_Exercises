using System.Collections.Generic;

/// <summary>
/// 所有业务逻辑系统的抽象基类，负责处理特定组件组合的实体。
/// 1. 关注点分离：每个系统只负责一个原子化的逻辑（如：移动、碰撞）。
/// 2. 查询优化：通过缓存查询结果（QueryCache）避免重复遍历全局实体列表。
/// 3. 无状态设计：系统本身不应存储实体状态，所有状态应存储在组件中。
/// </summary>
public abstract class SystemBase
{
    protected List<Entity> _entities;
    
    // 缓存Key的占位结构体
    private struct CacheKey<T1> {}
    private struct CacheKey<T1, T2> {}
    private struct CacheKey<T1, T2, T3> {}
    private struct CacheKey<T1, T2, T3, T4> {}
    private struct CacheKey<T1, T2, T3, T4,T5> {}
    
    public SystemBase(List<Entity> entities)
    {
        _entities = entities;
    }
    /// <summary>
    /// 每帧调用的主循环函数。
    /// </summary>
    /// <param name="deltaTime">自上一帧以来的增量时间（秒）。</param>
    public abstract void Update(float deltaTime);
    /// <summary>
    /// 高效筛选包含指定组件组合的实体列表。
    /// </summary>
    /// <typeparam name="T1">必须包含的组件类型 1</typeparam>
    /// <returns>返回符合条件的实体列表。注：该列表由 ECSManager 管理并从对象池中分配。</returns>
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
    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4,T5>() where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
    {
        var key = typeof(CacheKey<T1, T2, T3, T4,T5>);
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T1>() && entity.HasComponent<T2>() && entity.HasComponent<T3>() && entity.HasComponent<T4>()&& entity.HasComponent<T5>())
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