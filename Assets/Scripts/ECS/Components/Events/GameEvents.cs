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

