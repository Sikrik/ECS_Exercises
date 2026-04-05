public class InvincibleComponent : Component 
{
    public float RemainingTime;
}

public class SlowEffectComponent : Component 
{
    public float SlowRatio;
    public float RemainingDuration;
    public SlowEffectComponent(float r, float d) { SlowRatio = r; RemainingDuration = d; }
}

public class LifetimeComponent : Component 
{
    public float RemainingTime; // 子弹、特效等的生存计时
}

public class AIStateComponent : Component { public float CurrentCooldown; }

public class KnockbackComponent : Component 
{
    public float DirX, DirY, Speed, Timer;
}

public class HitRecoveryComponent : Component { public float Timer; }