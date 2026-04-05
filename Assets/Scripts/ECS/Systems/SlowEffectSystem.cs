using System.Collections.Generic;
using UnityEngine;

public class SlowEffectSystem : SystemBase
{
    public SlowEffectSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 筛选正在被减速的实体
        var slowedEntities = GetEntitiesWith<SlowEffectComponent>();
        
        for (int i = slowedEntities.Count - 1; i >= 0; i--)
        {
            var entity = slowedEntities[i];
            if (!entity.IsAlive) continue;

            var slow = entity.GetComponent<SlowEffectComponent>();
            slow.RemainingDuration -= deltaTime;
            
            if (slow.RemainingDuration <= 0)
            {
                // 减速结束，移除组件。速度恢复逻辑在 EnemyAISystem 中处理
                entity.RemoveComponent<SlowEffectComponent>();
                
                // 如果有附加特效，同步移除
                if (entity.HasComponent<AttachedVFXComponent>())
                {
                    var vfx = entity.GetComponent<AttachedVFXComponent>();
                    PoolManager.Instance.Despawn(vfx.EffectObject);
                    entity.RemoveComponent<AttachedVFXComponent>();
                }
            }
        }
    }
}