using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 高频事件特供对象池 (白嫖级 0 GC 优化)
// ==========================================
public static class EventPool 
{
    private static Stack<DamageTakenEventComponent> _damagePool = new Stack<DamageTakenEventComponent>();
    private static Stack<CollisionEventComponent> _collisionPool = new Stack<CollisionEventComponent>();

    // 【修改】增加 durationOverride 参数
    public static DamageTakenEventComponent GetDamageEvent(float amount, bool causeHitRecovery, float durationOverride = 0f)
    {
        var evt = _damagePool.Count > 0 ? _damagePool.Pop() : new DamageTakenEventComponent();
        evt.DamageAmount = amount;
        evt.CauseHitRecovery = causeHitRecovery; 
        evt.RecoveryDurationOverride = durationOverride; // 赋值时间覆盖
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
        evt.Source = null; 
        evt.Target = null; 
        _collisionPool.Push(evt);
    }
}