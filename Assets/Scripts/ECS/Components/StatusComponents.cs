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

public class AIStateComponent : Component { public float CurrentCooldown; }

public class KnockbackComponent : Component 
{
    public float DirX, DirY, Speed, Timer;
}

public class HitRecoveryComponent : Component { public float Timer; }