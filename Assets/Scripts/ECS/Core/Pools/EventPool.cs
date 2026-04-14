// 路径: Assets/Scripts/ECS/Core/Pools/EventPool.cs
using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 高频事件特供对象池 (白嫖级 0 GC 优化)
// ==========================================
public static class EventPool 
{
    private static Stack<DamageTakenEventComponent> _damagePool = new Stack<DamageTakenEventComponent>();
    private static Stack<CollisionEventComponent> _collisionPool = new Stack<CollisionEventComponent>();
    
    // 👇【新增 1】：冲刺开始事件的对象池栈
    private static Stack<DashStartedEventComponent> _dashStartedPool = new Stack<DashStartedEventComponent>();

    public static DamageTakenEventComponent GetDamageEvent(float amount, bool causeHitRecovery, float durationOverride = 0f)
    {
        var evt = _damagePool.Count > 0 ? _damagePool.Pop() : new DamageTakenEventComponent();
        evt.DamageAmount = amount;
        evt.CauseHitRecovery = causeHitRecovery; 
        evt.RecoveryDurationOverride = durationOverride; 
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

    // ==========================================
    // 👇【新增 2】：冲刺开始事件的 Get 和 Return 方法
    // ==========================================
    public static DashStartedEventComponent GetDashStartedEvent()
    {
        return _dashStartedPool.Count > 0 ? _dashStartedPool.Pop() : new DashStartedEventComponent();
    }

    public static void Return(DashStartedEventComponent evt)
    {
        _dashStartedPool.Push(evt);
    }
}