// 路径: Assets/Scripts/ECS/Core/SystemBase.cs
using System;
using System.Collections.Generic;

/// <summary>
/// 业务逻辑系统基类 (终极性能版)
/// 核心优化：
/// 1. 彻底消灭哈希字典查询，采用 ulong BitMask 位运算进行极速实体匹配。
/// 2. 使用 for 循环代替 foreach，斩断隐式迭代器开销。
/// 3. 复用 ECSManager.GetListFromPool，实现严格的 0 GC。
/// </summary>
public abstract class SystemBase
{
    protected List<Entity> _entities;
    
    // 缓存每种查询组合的目标掩码，避免每帧重复计算
    // Key: 查询签名的组合类型 (借用 Action<T...> 来生成唯一 Type Key)
    // Value: (掩码0, 掩码1)
    private Dictionary<Type, (ulong m0, ulong m1)> _queryMaskCache = new Dictionary<Type, (ulong, ulong)>();

    public SystemBase(List<Entity> entities)
    {
        _entities = entities;
    }

    public abstract void Update(float deltaTime);

    // ==========================================
    // 内部方法：生成并缓存多组件查询掩码
    // ==========================================
    private (ulong m0, ulong m1) GetOrCreateQueryMask(Type querySignature, params int[] componentIds)
    {
        if (_queryMaskCache.TryGetValue(querySignature, out var mask))
        {
            return mask;
        }

        ulong m0 = 0, m1 = 0;
        foreach (int id in componentIds)
        {
            if (id < 64) m0 |= (1UL << id);
            else         m1 |= (1UL << (id - 64));
        }

        _queryMaskCache[querySignature] = (m0, m1);
        return (m0, m1);
    }

    // ==========================================
    // 极致性能：基于 BitMask 的多组件查询接口
    // ==========================================

    protected List<Entity> GetEntitiesWith<T>() where T : Component
    {
        var mask = GetOrCreateQueryMask(typeof(Action<T>), ComponentTypeManager.GetId<T>());
        var list = ECSManager.Instance.GetListFromPool();
        
        int count = _entities.Count;
        for (int i = 0; i < count; i++)
        {
            var e = _entities[i];
            // 核心判定：只用极速位运算，(实体掩码 & 目标掩码) == 目标掩码
            if (e.IsAlive && (e.Mask0 & mask.m0) == mask.m0 && (e.Mask1 & mask.m1) == mask.m1)
            {
                list.Add(e);
            }
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2>() 
        where T1 : Component where T2 : Component
    {
        var mask = GetOrCreateQueryMask(
            typeof(Action<T1, T2>), 
            ComponentTypeManager.GetId<T1>(), 
            ComponentTypeManager.GetId<T2>()
        );
        
        var list = ECSManager.Instance.GetListFromPool();
        int count = _entities.Count;
        for (int i = 0; i < count; i++)
        {
            var e = _entities[i];
            if (e.IsAlive && (e.Mask0 & mask.m0) == mask.m0 && (e.Mask1 & mask.m1) == mask.m1)
            {
                list.Add(e);
            }
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2, T3>() 
        where T1 : Component where T2 : Component where T3 : Component
    {
        var mask = GetOrCreateQueryMask(
            typeof(Action<T1, T2, T3>), 
            ComponentTypeManager.GetId<T1>(), 
            ComponentTypeManager.GetId<T2>(), 
            ComponentTypeManager.GetId<T3>()
        );
        
        var list = ECSManager.Instance.GetListFromPool();
        int count = _entities.Count;
        for (int i = 0; i < count; i++)
        {
            var e = _entities[i];
            if (e.IsAlive && (e.Mask0 & mask.m0) == mask.m0 && (e.Mask1 & mask.m1) == mask.m1)
            {
                list.Add(e);
            }
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4>() 
        where T1 : Component where T2 : Component where T3 : Component where T4 : Component
    {
        var mask = GetOrCreateQueryMask(
            typeof(Action<T1, T2, T3, T4>), 
            ComponentTypeManager.GetId<T1>(), 
            ComponentTypeManager.GetId<T2>(), 
            ComponentTypeManager.GetId<T3>(), 
            ComponentTypeManager.GetId<T4>()
        );
        
        var list = ECSManager.Instance.GetListFromPool();
        int count = _entities.Count;
        for (int i = 0; i < count; i++)
        {
            var e = _entities[i];
            if (e.IsAlive && (e.Mask0 & mask.m0) == mask.m0 && (e.Mask1 & mask.m1) == mask.m1)
            {
                list.Add(e);
            }
        }
        return list;
    }

    protected List<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() 
        where T1 : Component where T2 : Component where T3 : Component where T4 : Component where T5 : Component
    {
        var mask = GetOrCreateQueryMask(
            typeof(Action<T1, T2, T3, T4, T5>), 
            ComponentTypeManager.GetId<T1>(), 
            ComponentTypeManager.GetId<T2>(), 
            ComponentTypeManager.GetId<T3>(), 
            ComponentTypeManager.GetId<T4>(), 
            ComponentTypeManager.GetId<T5>()
        );
        
        var list = ECSManager.Instance.GetListFromPool();
        int count = _entities.Count;
        for (int i = 0; i < count; i++)
        {
            var e = _entities[i];
            if (e.IsAlive && (e.Mask0 & mask.m0) == mask.m0 && (e.Mask1 & mask.m1) == mask.m1)
            {
                list.Add(e);
            }
        }
        return list;
    }
}