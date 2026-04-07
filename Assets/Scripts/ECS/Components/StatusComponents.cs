public class InvincibleComponent : Component 
{
    public float Duration;
}

public class SlowEffectComponent : Component 
{
    public float SlowRatio;
    public float Duration;
    public SlowEffectComponent(float r, float d) { SlowRatio = r; Duration = d; }
}

public class LifetimeComponent : Component 
{
    public float Duration; // 子弹、特效等的生存计时
}

public class KnockbackComponent : Component 
{
    public float DirX, DirY, Speed, Timer;
}

public class HitRecoveryComponent : Component { public float Timer; }

/// <summary>
/// 每帧动态计算的状态汇总结果
/// </summary>
public class StatusSummaryComponent : Component
{
    public bool CanMove = true;
    public float SpeedMultiplier = 1f;
}
/// <summary>
/// 待销毁标记：被贴上此标签的实体，将在帧末被统一回收。
/// </summary>
public class PendingDestroyComponent : Component { }