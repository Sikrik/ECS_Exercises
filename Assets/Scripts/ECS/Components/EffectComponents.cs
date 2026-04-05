using UnityEngine;

// --- 基础战斗能力 ---

public class DamageComponent : Component {
    public float Value;
    public DamageComponent(float v) => Value = v;
}

public class AOEComponent : Component {
    public float Radius;
    public float Damage;
    public AOEComponent(float r, float d) { Radius = r; Damage = d; }
}

public class ChainComponent : Component {
    public int MaxTargets;
    public float Range;
    public float Damage;
    public ChainComponent(int count, float r, float d) { MaxTargets = count; Range = r; Damage = d; }
}

// --- 状态效果 (Buff/Debuff) ---

/// <summary>
/// 减速状态组件：当实体被减速时挂载
/// </summary>
public class SlowEffectComponent : Component 
{
    public float SlowRatio;         // 减速比例
    public float RemainingDuration; // 持续时间
    
    public SlowEffectComponent(float slowRatio, float duration)
    {
        SlowRatio = slowRatio;
        RemainingDuration = duration;
    }
}
public class LightningVFXComponent : Component 
{
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float Duration;
    public float Timer;

    public LightningVFXComponent(Vector3 s, Vector3 e, float d = 0.15f) 
    { 
        StartPos = s; EndPos = e; Duration = d; Timer = 0;
    }
}

/// <summary>
/// 附加视觉特效标记：用于绑定除主视图外的额外特效（如减速时的冰冻特效）
/// </summary>
public class AttachedVFXComponent : Component
{
    public GameObject EffectObject;
    public AttachedVFXComponent(GameObject go) => EffectObject = go;
}

