using UnityEngine;
/// <summary>
/// 减速效果组件，标记被减速的敌人，存储减速效果数据
/// </summary>
public class SlowEffectComponent : Component 
{
    /// <summary>减速比例（0.5代表速度变为原来的50%）</summary>
    public float SlowRatio;
    /// <summary>减速效果剩余持续时间</summary>
    public float RemainingDuration;
    /// <summary>减速持续特效的GameObject实例，用于特效结束时销毁</summary>
    public GameObject EffectObject;
    
    public SlowEffectComponent(float slowRatio, float duration)
    {
        SlowRatio = slowRatio;
        RemainingDuration = duration;
    }
}