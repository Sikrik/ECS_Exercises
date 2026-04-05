using UnityEngine;

/// <summary>
/// 【重要性：★★★★★ - 核心战斗组件】
/// 基础伤害能力组件，存储伤害数值
/// 这是所有攻击和战斗系统的最基本单元，其他高级效果（AOE、闪电链等）都基于此组件
/// 几乎所有造成伤害的实体都需要此组件，是战斗系统的基石
/// </summary>
public class DamageComponent : Component {
    /// <summary>伤害数值，用于计算目标受到的伤害量</summary>
    public float Value;
    public DamageComponent(float v) => Value = v;
}

/// <summary>
/// 爆炸(AOE)能力组件，存储范围伤害的参数
/// 提供群体伤害能力，是游戏中重要的清场和控制手段
/// 依赖DamageComponent的基础伤害概念，但增加了范围检测逻辑
/// </summary>
public class AOEComponent : Component {
    /// <summary>爆炸影响半径，范围内的所有敌人都会受到伤害</summary>
    public float Radius;
    /// <summary>AOE伤害数值，对范围内每个敌人造成的伤害</summary>
    public float Damage;
    public AOEComponent(float r, float d) { Radius = r; Damage = d; }
}

/// <summary>
/// 闪电链能力组件，存储连锁闪电的参数
/// 提供多目标打击能力，可以在多个敌人之间跳跃造成伤害
/// 是实现策略性战斗的重要组件，适合处理密集敌群
/// </summary>
public class ChainComponent : Component {
    /// <summary>最大连锁目标数量，决定闪电可以跳跃多少次</summary>
    public int MaxTargets;
    /// <summary>连锁搜索范围，在此范围内寻找下一个目标</summary>
    public float Range;
    /// <summary>每次连锁造成的伤害值</summary>
    public float Damage;
    public ChainComponent(int count, float r, float d) { MaxTargets = count; Range = r; Damage = d; }
}

/// <summary>
/// 减速效果组件，存储目标的减速状态信息
/// 提供战场控制能力，降低敌人移动速度，为玩家创造优势
/// 包含持续时间管理和特效引用，需要定时更新和清理
/// </summary>
public class SlowEffectComponent : Component 
{
    /// <summary>减速比例（0.5代表速度变为原来的50%）</summary>
    public float SlowRatio;
    /// <summary>减速效果剩余持续时间，倒计时结束后移除减速效果</summary>
    public float RemainingDuration;
    /// <summary>减速持续特效的GameObject实例，用于特效结束时销毁</summary>
    public GameObject EffectObject;
    
    public SlowEffectComponent(float slowRatio, float duration)
    {
        SlowRatio = slowRatio;
        RemainingDuration = duration;
    }
}

/// <summary>
/// 闪电链视觉数据组件，仅用于渲染闪电特效
/// 不包含任何游戏逻辑，纯粹为了视觉表现服务
/// 在闪电链效果播放期间临时存在，播放完成后立即销毁
/// </summary>
public class LightningVFXComponent : Component {
    /// <summary>闪电起始位置，通常是上一个目标的位置</summary>
    public Vector3 StartPos;
    /// <summary>闪电结束位置，通常是当前目标的位置</summary>
    public Vector3 EndPos;
    /// <summary>闪电特效的总持续时间</summary>
    public float Duration;
    /// <summary>闪电特效已播放的时间，用于判断何时销毁</summary>
    public float Timer;
    public LightningVFXComponent(Vector3 s, Vector3 e, float d = 0.15f) { StartPos = s; EndPos = e; Duration = d; }
}
