using System.Collections.Generic;

/// <summary>
/// 实体池：负责 Entity 对象的复用，防止频繁 new Entity 导致 GC
/// </summary>
public static class EntityPool
{
    private static Stack<Entity> _pool = new Stack<Entity>();

    public static Entity Get()
    {
        Entity e = _pool.Count > 0 ? _pool.Pop() : new Entity();
        e.IsAlive = true;
        return e;
    }

    public static void Return(Entity e)
    {
        e.IsAlive = false;
        e.ClearComponents(); // 回收前彻底清空组件字典，防止引用残留
        
        if (!_pool.Contains(e))
        {
            _pool.Push(e);
        }
    }
}