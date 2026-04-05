using System.Collections.Generic;
/// <summary>
/// ECS架构中所有系统的抽象基类
/// 系统负责处理拥有特定组件的实体的业务逻辑，实现数据与逻辑的分离
/// 优化后：添加查询缓存，减少重复遍历，消除GC分配
/// </summary>
public abstract class SystemBase
{
    /// <summary>
    /// 系统可访问的所有实体列表
    /// </summary>
    protected List<Entity> _entities;
    
    // 缓存Key的占位结构体，用于为不同的组件组合生成唯一的Type作为缓存Key
    private struct CacheKey<T1> {}
    private struct CacheKey<T1, T2> {}
    private struct CacheKey<T1, T2, T3> {}
    private struct CacheKey<T1, T2, T3, T4> {}
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    public SystemBase(List<Entity> entities)
    {
        _entities = entities;
    }
    /// <summary>
    /// 每帧更新
    /// </summary>
    public abstract void Update(float deltaTime);
    
    /// <summary>
    /// 获取拥有指定组件的所有实体，带缓存优化
    /// </summary>
    protected List<Entity> GetEntitiesWith<T>() where T : Component
    {
        var key = typeof(CacheKey<T>);
        // 先查缓存，命中则直接返回
        if (ECSManager.Instance.QueryCache.TryGetValue(key, out var list))
        {
            return list;
        }
        
        // 缓存未命中，从对象池获取List，填充数据
        list = ECSManager.Instance.GetListFromPool();
        foreach (var entity in _entities)
        {
            if (entity.IsAlive && entity.HasComponent<T>())
            {
                list.Add(entity);
            }
        }
        
        // 存入缓存，供这一帧内的其他查询复用
        ECSManager.Instance.QueryCache[key] = list;
        return list;
    }
    
    /// <summary>
    /// 获取拥有指定两个组件的所有实体，带缓存优化
    /// </summary>
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
    
    /// <summary>
    /// 获取拥有指定三个组件的所有实体，带缓存优化
    /// </summary>
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
    
    /// <summary>
    /// 获取拥有指定四个组件的所有实体，带缓存优化
    /// </summary>
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
    
    /// <summary>
    /// 将查询列表归还到对象池，减少GC分配，适配协同文档的新代码
    /// </summary>
    protected void ReturnListToPool(List<Entity> list)
    {
        ECSManager.Instance.ReturnListToPool(list);
    }
}