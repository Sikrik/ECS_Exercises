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

/// <summary>
/// 附加视觉特效标记：用于绑定除主视图外的额外特效（如减速时的冰冻特效）
/// </summary>
public class AttachedVFXComponent : Component
{
    public GameObject EffectObject;
    public AttachedVFXComponent(GameObject go) => EffectObject = go;
}

/// <summary>
/// 闪电链视觉数据组件：仅用于存储渲染闪电所需的位置和时间数据
/// 由 BulletEffectSystem 创建，由 LightningRenderSystem 使用并销毁
/// </summary>
public class LightningVFXComponent : Component 
{
    public Vector3 StartPos; // 闪电起始点
    public Vector3 EndPos;   // 闪电结束点
    public float Duration;   // 特效持续时间
    public float Timer;      // 已播放时间计时器

    public LightningVFXComponent(Vector3 s, Vector3 e, float d = 0.15f) 
    { 
        StartPos = s; 
        EndPos = e; 
        Duration = d; 
        Timer = 0;
    }
}