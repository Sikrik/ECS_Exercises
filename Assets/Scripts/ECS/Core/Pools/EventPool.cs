using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 高频事件特供对象池 (白嫖级 0 GC 优化)
// ==========================================
public static class EventPool 
{
    private static Stack<DamageTakenEventComponent> _damagePool = new Stack<DamageTakenEventComponent>();
    private static Stack<CollisionEventComponent> _collisionPool = new Stack<CollisionEventComponent>();

    // 【修改】增加 causeHitRecovery 参数
    public static DamageTakenEventComponent GetDamageEvent(float amount, bool causeHitRecovery)
    {
        var evt = _damagePool.Count > 0 ? _damagePool.Pop() : new DamageTakenEventComponent();
        evt.DamageAmount = amount;
        evt.CauseHitRecovery = causeHitRecovery; // 赋值硬直标识
        return evt;
    }

    public static void Return(DamageTakenEventComponent evt) => _damagePool.Push(evt);

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
        // 清空所有引用，防止实体被回收后这里还拿捏着引用导致内存泄漏
        evt.Source = null; 
        evt.Target = null; 
        _collisionPool.Push(evt);
    }
}