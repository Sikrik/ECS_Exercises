using UnityEngine;

/// <summary>
/// 伤害组件，存储攻击造成的伤害数值
/// </summary>
public class DamageComponent : Component 
{
    public float Value;
    public DamageComponent(float v) => Value = v;
}

/// <summary>
/// 悬赏组件，存储实体死亡时提供的分数奖励
/// </summary>
public class BountyComponent : Component 
{
    public int Score;
    public BountyComponent(int score) => Score = score;
}

/// <summary>
/// 受击硬直配置组件，存储实体默认的硬直持续时间
/// </summary>
public class HitRecoveryStatsComponent : Component 
{
    public float Duration;
    public HitRecoveryStatsComponent(float duration) => Duration = duration;
}

/// <summary>
/// 碰撞反弹强度组件，存储实体在发生碰撞时产生的反弹力度数值
/// </summary>
public class BounceForceComponent : Component 
{
    public float Value;
    public BounceForceComponent(float v) => Value = v;
}

/// <summary>
/// 生命值组件，存储实体的当前生命值和最大生命值
/// </summary>
public class HealthComponent : Component 
{
    public float CurrentHealth;
    public float MaxHealth;

    public HealthComponent(float maxHealth) { MaxHealth = maxHealth; CurrentHealth = maxHealth; }
}

/// <summary>
/// 存储碰撞时对目标造成的反馈效果
/// </summary>
public class ImpactFeedbackComponent : Component 
{
    public bool CauseBounce;      
    public bool CauseHitRecovery; 
    // 新增：用于覆盖怪物默认的硬直时间 (由升级项提供)
    public float HitRecoveryDurationOverride; 

    public ImpactFeedbackComponent(bool bounce, bool recovery, float durationOverride = 0f) 
    {
        CauseBounce = bounce;
        CauseHitRecovery = recovery;
        HitRecoveryDurationOverride = durationOverride;
    }
}