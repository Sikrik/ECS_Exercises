using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理碰撞事件组件，用于在实体间传递碰撞信息
/// </summary>
public class CollisionEventComponent : Component 
{
    public Entity Source;
    public Entity Target;
    public Vector2 Normal;
    
    public CollisionEventComponent() { }

    public CollisionEventComponent(Entity src, Entity target, Vector2 normal) 
    {
        Source = src; 
        Target = target; 
        Normal = normal;
    }
}

/// <summary>
/// 加分事件组件，用于触发分数增加逻辑
/// </summary>
public class ScoreEventComponent : Component
{
    public int Amount;
    public ScoreEventComponent(int amount) => Amount = amount;
}

/// <summary>
/// 伤害承受事件组件，用于在实体受到伤害时传递伤害信息
/// </summary>
public class DamageTakenEventComponent : Component
{
    public float DamageAmount;
    public bool CauseHitRecovery;
    // 新增：传递给受击者的最终硬直覆盖时间
    public float RecoveryDurationOverride; 
    
    public DamageTakenEventComponent() { }
    
    public DamageTakenEventComponent(float amt, bool causeRecovery, float durationOverride = 0f) 
    { 
        DamageAmount = amt; 
        CauseHitRecovery = causeRecovery;
        RecoveryDurationOverride = durationOverride;
    }
}