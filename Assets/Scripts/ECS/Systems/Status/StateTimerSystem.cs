using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态计时系统：统一管理各种状态组件的生命周期（如硬直、击退时间）。
/// </summary>
public class StateTimerSystem : SystemBase
{
    public StateTimerSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 处理受击硬直计时
        var recoveries = GetEntitiesWith<HitRecoveryComponent>();
        for (int i = recoveries.Count - 1; i >= 0; i--)
        {
            var hr = recoveries[i].GetComponent<HitRecoveryComponent>();
            hr.Timer -= deltaTime;
            if (hr.Timer <= 0) recoveries[i].RemoveComponent<HitRecoveryComponent>();
        }

        // 2. 处理击退组件计时（如果以后添加 KnockbackComponent 存储位移逻辑）
        var knockbacks = GetEntitiesWith<KnockbackComponent>();
        for (int i = knockbacks.Count - 1; i >= 0; i--)
        {
            var kb = knockbacks[i].GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            if (kb.Timer <= 0) knockbacks[i].RemoveComponent<KnockbackComponent>();
        }
        
        // 3. 处理减速效果计时（原本在 SlowEffectSystem，现在也可以合并到这里）
    }
}

public class StatusGatherSystem : SystemBase
{
    public StatusGatherSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 找到所有带有汇总组件的实体
        var entities = GetEntitiesWith<StatusSummaryComponent>();
        
        foreach (var e in entities)
        {
            var summary = e.GetComponent<StatusSummaryComponent>();
            
            // 1. 每帧重置为初始状态
            summary.CanMove = true;
            summary.SpeedMultiplier = 1f;

            // 2. 检查硬控：如果有硬直或击退，标记为无法移动
            if (e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                summary.CanMove = false;
            }

            // 3. 检查软控：如果有减速，计算最终速度倍率
            if (e.HasComponent<SlowEffectComponent>())
            {
                summary.SpeedMultiplier *= (1f - e.GetComponent<SlowEffectComponent>().SlowRatio);
            }
        }
    }
}