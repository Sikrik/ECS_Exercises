// Assets/Scripts/ECS/Systems/Status/StatusGatherSystem.cs

using System.Collections.Generic;

public class StatusGatherSystem : SystemBase
{
    public StatusGatherSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 现在只查询拥有速度组件的实体
        var entities = GetEntitiesWith<SpeedComponent>();
        
        foreach (var e in entities)
        {
            var speed = e.GetComponent<SpeedComponent>();
            
            // 1. 默认每一帧开始时，速度恢复为基础配置值
            float finalSpeed = speed.BaseSpeed;

            // 2. 逻辑简化：检查“硬控”（硬直或击退）
            // 如果处于受击硬直或正在被击退，实时速度直接归零
            if (e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                finalSpeed = 0;
            }
            // 3. 检查“软控”（减速）
            // 如果没有硬控，再看看有没有减速效果
            else if (e.HasComponent<SlowEffectComponent>())
            {
                finalSpeed *= (1f - e.GetComponent<SlowEffectComponent>().SlowRatio);
            }

            // 4. 直接写入结果，后续系统只需读 CurrentSpeed 即可
            speed.CurrentSpeed = finalSpeed;
        }
    }
}