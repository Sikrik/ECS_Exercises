public class DamageComponent : Component { public float Value; public DamageComponent(float v) => Value = v; }
// Assets/Scripts/ECS/Components/ActorComponents.cs

/// <summary>
/// 悬赏组件：存储实体死亡时提供的分数
/// </summary>
public class BountyComponent : Component 
{
    public int Score;
    public BountyComponent(int score) => Score = score;
}

/// <summary>
/// 受击硬直配置组件：存储该类实体默认的硬直时间
/// </summary>
public class HitRecoveryStatsComponent : Component 
{
    public float Duration;
    public HitRecoveryStatsComponent(float duration) => Duration = duration;
}
/// <summary>
/// 碰撞反弹强度组件：存储该实体在发生碰撞时产生的反弹力度数值
/// </summary>
public class BounceForceComponent : Component 
{
    public float Value;
    public BounceForceComponent(float v) => Value = v;
}