using System.Collections.Generic;
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