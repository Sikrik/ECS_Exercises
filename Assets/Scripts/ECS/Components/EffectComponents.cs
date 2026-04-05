using UnityEngine;

// --- 基础战斗能力组件 ---

/// <summary>单体伤害组件</summary>
public class DamageComponent : Component { 
    public float Value; 
    public DamageComponent(float v) => Value = v; 
}

/// <summary>爆炸伤害组件 (AOE)</summary>
public class AOEComponent : Component { 
    public float Radius; 
    public float Damage; 
    public AOEComponent(float r, float d) { Radius = r; Damage = d; } 
}

/// <summary>闪电链弹射组件</summary>
public class ChainComponent : Component { 
    public int MaxTargets; 
    public float Range; 
    public float Damage; 
    public ChainComponent(int m, float r, float d) { MaxTargets = m; Range = r; Damage = d; } 
}

// --- 状态效果组件 ---

/// <summary>减速状态组件</summary>
public class SlowEffectComponent : Component { 
    public float SlowRatio;         
    public float RemainingDuration; 
    public Color OriginalColor = Color.clear; // 新增：保存初始颜色
    public SlowEffectComponent(float r, float d) { SlowRatio = r; RemainingDuration = d; } 
}

// --- 视觉特效组件 ---

/// <summary>闪电链渲染数据</summary>
public class LightningVFXComponent : Component {
    public Vector3 StartPos, EndPos;
    public float Duration, Timer;
    public LightningVFXComponent(Vector3 s, Vector3 e, float d = 0.15f) { 
        StartPos = s; EndPos = e; Duration = d; Timer = 0; 
    }
}

/// <summary>附加视觉对象标记 (用于绑定跟随实体的特效)</summary>
public class AttachedVFXComponent : Component { 
    public GameObject EffectObject; 
    public AttachedVFXComponent(GameObject go) => EffectObject = go; 
}