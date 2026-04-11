using System.Collections.Generic;
using UnityEngine;

public class SlowEffectSystem : SystemBase
{
    private readonly Color _slowColor = new Color(0.5f, 0.8f, 1f, 1f); 

    public SlowEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<SlowEffectComponent>();

        foreach (var entity in entities)
        {
            var slow = entity.GetComponent<SlowEffectComponent>();
            slow.Duration -= deltaTime;

            if (slow.Duration > 0)
            {
                if (!entity.HasComponent<ColorTintComponent>())
                    entity.AddComponent(new ColorTintComponent(_slowColor));
            }
            else
            {
                entity.RemoveComponent<ColorTintComponent>();

                // 【重构】：不再直接 Destroy，而是打上单帧标签交由表现层处理
                if (entity.HasComponent<AttachedVFXComponent>())
                {
                    entity.AddComponent(new PendingVFXDestroyTag());
                }
                
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}