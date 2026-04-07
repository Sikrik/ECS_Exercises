using System.Collections.Generic;
public class StatusGatherSystem : SystemBase
{
    public StatusGatherSystem(List<Entity> entities) : base(entities) { }

    // Assets/Scripts/ECS/Systems/Status/StatusGatherSystem.cs

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<StatusSummaryComponent, SpeedComponent>();
    
        foreach (var e in entities)
        {
            var summary = e.GetComponent<StatusSummaryComponent>();
            var speed = e.GetComponent<SpeedComponent>();
        
            // 1. 重置乘法器
            summary.SpeedMultiplier = 1f;

            // 2. 检查减速效果
            if (e.HasComponent<SlowEffectComponent>())
            {
                summary.SpeedMultiplier *= (1f - e.GetComponent<SlowEffectComponent>().SlowRatio);
            }

            // 3. 最终计算：将结果写入 SpeedComponent
            // 这样后续的移动系统只需要读 CurrentSpeed，不需要知道有没有被减速
            speed.CurrentSpeed = speed.BaseSpeed * summary.SpeedMultiplier;
        }
    }
}