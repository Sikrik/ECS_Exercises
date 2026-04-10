/// <summary>
/// 伤害组件，存储攻击造成的伤害数值
/// </summary>
public class DamageComponent : Component 
{
    /// <summary>
    /// 伤害数值
    /// </summary>
    public float Value;

    /// <summary>
    /// 初始化伤害组件
    /// </summary>
    /// <param name="v">伤害数值</param>
    public DamageComponent(float v) => Value = v;
}

/// <summary>
/// 悬赏组件，存储实体死亡时提供的分数奖励
/// </summary>
public class BountyComponent : Component 
{
    /// <summary>
    /// 击杀该实体可获得的分数值
    /// </summary>
    public int Score;

    /// <summary>
    /// 初始化悬赏组件
    /// </summary>
    /// <param name="score">悬赏分数值</param>
    public BountyComponent(int score) => Score = score;
}

/// <summary>
/// 受击硬直配置组件，存储实体默认的硬直持续时间
/// </summary>
public class HitRecoveryStatsComponent : Component 
{
    /// <summary>
    /// 硬直持续时间，单位：秒
    /// </summary>
    public float Duration;

    /// <summary>
    /// 初始化受击硬直配置组件
    /// </summary>
    /// <param name="duration">硬直持续时间</param>
    public HitRecoveryStatsComponent(float duration) => Duration = duration;
}

/// <summary>
/// 碰撞反弹强度组件，存储实体在发生碰撞时产生的反弹力度数值
/// </summary>
public class BounceForceComponent : Component 
{
    /// <summary>
    /// 碰撞反弹力度值，用于计算反弹速度
    /// </summary>
    public float Value;

    /// <summary>
    /// 初始化碰撞反弹强度组件
    /// </summary>
    /// <param name="v">反弹力度值</param>
    public BounceForceComponent(float v) => Value = v;
}

/// <summary>
/// 生命值组件，存储实体的当前生命值和最大生命值
/// </summary>
public class HealthComponent : Component 
{
    /// <summary>
    /// 当前生命值
    /// </summary>
    public float CurrentHealth;

    /// <summary>
    /// 最大生命值上限
    /// </summary>
    public float MaxHealth;

    /// <summary>
    /// 初始化生命值组件，当前生命值和最大生命值均设为指定值
    /// </summary>
    /// <param name="maxHealth">最大生命值</param>
    public HealthComponent(float maxHealth) { MaxHealth = maxHealth; CurrentHealth = maxHealth; }
}