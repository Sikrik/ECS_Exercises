// Assets/Scripts/ECS/Systems/SlowEffectSystem.cs 完整修复版
using System.Collections.Generic;
using UnityEngine;

public class SlowEffectSystem : SystemBase
{
    public SlowEffectSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        var slowedEntities = GetEntitiesWith<SlowEffectComponent>();
        
        // 安全检查：防止列表本身为 null
        if (slowedEntities == null) return; 
        
        for (int i = slowedEntities.Count - 1; i >= 0; i--)
        {
            var entity = slowedEntities[i];
            // 检查实体是否存活
            if (entity == null || !entity.IsAlive) continue;

            var slowEffect = entity.GetComponent<SlowEffectComponent>();
            
            // 关键修复：检查组件是否存在，防止缓存导致的 null
            if (slowEffect == null) continue; 
            
            slowEffect.RemainingDuration -= deltaTime;
            
            if (slowEffect.RemainingDuration <= 0)
            {
                if (slowEffect.EffectObject != null)
                {
                    Object.Destroy(slowEffect.EffectObject);
                }
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}