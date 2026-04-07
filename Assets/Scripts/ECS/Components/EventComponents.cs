using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹命中事件组件
/// 当子弹碰撞系统检测到接触时，会给子弹挂载此组件。
/// BulletEffectSystem 会根据此组件的信息分发伤害。
/// </summary>
public class BulletHitEventComponent : Component 
{
    public Entity Target; // 命中的目标实体

    public BulletHitEventComponent(Entity target)
    {
        Target = target;
    }
}

/// <summary>
/// 经验值收集事件 (扩展用)
/// </summary>
public class ExpCollectEventComponent : Component
{
    public int Amount;
    public ExpCollectEventComponent(int amount) => Amount = amount;
}

/// <summary>
/// 物理碰撞事件组件
/// </summary>
public class CollisionEventComponent : Component 
{
    public Entity Source; // 谁撞的
    public Entity Target; // 撞了谁
    public Vector2 Normal; // 碰撞法线（用于计算反弹方向/击退方向）
    
    // 👇 专门给对象池准备的无参构造函数
    public CollisionEventComponent() { }

    public CollisionEventComponent(Entity src, Entity target, Vector2 normal) 
    {
        Source = src; 
        Target = target; 
        Normal = normal;
    }
}

/// <summary>
/// 加分事件组件
/// </summary>
public class ScoreEventComponent : Component
{
    public int Amount;
    public ScoreEventComponent(int amount) => Amount = amount;
}

/// <summary>
/// 目标受到伤害后的瞬时事件组件
/// </summary>
public class DamageTakenEventComponent : Component
{
    public float DamageAmount;
    
    // 👇 专门给对象池准备的无参构造函数
    public DamageTakenEventComponent() { }
    
    public DamageTakenEventComponent(float amt) => DamageAmount = amt;
}

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