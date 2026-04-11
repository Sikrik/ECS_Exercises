using System.Collections.Generic;

/// <summary>
/// 减速弹反应系统（原子化）
/// </summary>
public class SlowBulletReactionSystem : SystemBase
{
    public SlowBulletReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source;
            var target = evt.Target;

            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<SlowEffectComponent>()) continue;
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            var bSlow = bullet.GetComponent<SlowEffectComponent>();
            var tSlow = target.GetComponent<SlowEffectComponent>();

            // 状态叠加或刷新
            if (tSlow != null)
            {
                tSlow.Duration = bSlow.Duration; 
            }
            else
            {
                // 赋予逻辑减速状态
                target.AddComponent(new SlowEffectComponent(bSlow.SlowRatio, bSlow.Duration));
                
                // 抛出表现层意图：在目标身上挂载减速冰冻特效
                Entity vfxEvent = ECSManager.Instance.CreateEntity();
                vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                    VFXType = "SlowVFX", 
                    AttachTarget = target 
                });
            }
        }
        ReturnListToPool(hitEvents);
    }
}