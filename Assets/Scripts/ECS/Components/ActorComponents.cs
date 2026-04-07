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