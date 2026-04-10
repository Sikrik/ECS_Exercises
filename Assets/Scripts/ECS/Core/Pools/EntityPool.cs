using System.Collections.Generic;

/// <summary>
/// 实体池：负责 Entity 对象的复用，防止频繁 new Entity 导致 GC
/// </summary>
public static class EntityPool
{
    // 预分配容量，假设同屏活跃实体较多，减少初期堆内存重新分配和扩容开销
    private static Stack<Entity> _pool = new Stack<Entity>(2000);

    public static Entity Get()
    {
        Entity e = _pool.Count > 0 ? _pool.Pop() : new Entity();
        e.IsAlive = true;
        return e;
    }

    public static void Return(Entity e)
    {
        // 【核心优化】：利用 IsAlive 状态判断是否已回收，完美替代 O(N) 的 _pool.Contains(e)
        // 从而将实体回收的复杂度从 O(N) 降至真正的 O(1)
        if (!e.IsAlive) return; 

        e.IsAlive = false;
        e.ClearComponents(); // 回收前彻底清空组件字典，防止引用残留
        
        _pool.Push(e);
    }
}