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
public class KnockbackComponent : Component 
{
    public float DirX, DirY, Speed, Timer;
}

public class HitRecoveryComponent : Component { public float Timer; }