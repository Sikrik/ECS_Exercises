using System.Collections.Generic;
using UnityEngine;
// ==========================================
// 高频事件特供对象池 (白嫖级 0 GC 优化)
// ==========================================
public static class EventPool 
{
    private static Stack<DamageTakenEventComponent> _damagePool = new Stack<DamageTakenEventComponent>();
    private static Stack<CollisionEventComponent> _collisionPool = new Stack<CollisionEventComponent>();

    public static DamageTakenEventComponent GetDamageEvent(float amount)
    {
        var evt = _damagePool.Count > 0 ? _damagePool.Pop() : new DamageTakenEventComponent();
        evt.DamageAmount = amount;
        return evt;
    }

    public static void Return(DamageTakenEventComponent evt) => _damagePool.Push(evt);

    // 👇 完美匹配你新的 CollisionEventComponent 属性
    public static CollisionEventComponent GetCollisionEvent(Entity src, Entity target, Vector2 normal)
    {
        var evt = _collisionPool.Count > 0 ? _collisionPool.Pop() : new CollisionEventComponent();
        evt.Source = src;
        evt.Target = target;
        evt.Normal = normal;
        return evt;
    }

    public static void Return(CollisionEventComponent evt)
    {
        // 👇 清空所有引用，防止实体被回收后这里还拿捏着引用导致内存泄漏
        evt.Source = null; 
        evt.Target = null; 
        _collisionPool.Push(evt);
    }
}