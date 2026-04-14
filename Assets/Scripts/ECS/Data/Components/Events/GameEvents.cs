// 路径: Assets/Scripts/ECS/Data/Components/Events/GameEvents.cs
using UnityEngine;

/// <summary>
/// 物理碰撞事件
/// </summary>
public class CollisionEventComponent : Component, IPooledEvent 
{
    public Entity Source;
    public Entity Target;
    public Vector2 Normal;
    
    public void Clear() 
    {
        Source = null; 
        Target = null; 
        Normal = Vector2.zero;
    }
}

/// <summary>
/// 伤害承受事件
/// </summary>
public class DamageTakenEventComponent : Component, IPooledEvent
{
    public float DamageAmount;
    public bool CauseHitRecovery;
    public float RecoveryDurationOverride; 
    
    public void Clear()
    {
        DamageAmount = 0;
        CauseHitRecovery = false;
        RecoveryDurationOverride = 0;
    }
}

/// <summary>
/// 伤害事件
/// </summary>
public class DamageEventComponent : Component, IPooledEvent
{
    public float DamageAmount;
    public Entity Source; 
    public bool IsCritical;

    public void Clear()
    {
        DamageAmount = 0;
        Source = null; // 【关键】释放实体引用
        IsCritical = false;
    }
}

/// <summary>
/// 冲刺开始瞬时事件
/// </summary>
public class DashStartedEventComponent : Component, IPooledEvent
{
    public void Clear() { } // 没有数据，直接留空即可
}

/// <summary>
/// 加分事件
/// </summary>
public class ScoreEventComponent : Component, IPooledEvent
{
    public int Amount;
    public void Clear() { Amount = 0; }
}