using System.Collections.Generic;

/// <summary>
/// 减速弹反应系统（原子化）
/// 修复说明：增加了对 BulletTag 的严格校验，防止携带减速组件的怪物在互相碰撞时引发“冰霜病毒传染”Bug。
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

            // ==========================================
            // 【核心修复】：追加了 !bullet.HasComponent<BulletTag>() 的身份校验
            // 确保只有真正的“子弹”才能触发减速效果，防止怪物传怪物
            // ==========================================
            if (bullet == null || !bullet.IsAlive || 
                !bullet.HasComponent<BulletTag>() || 
                !bullet.HasComponent<SlowEffectComponent>()) 
            {
                continue;
            }

            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) 
            {
                continue;
            }

            var bSlow = bullet.GetComponent<SlowEffectComponent>();
            var tSlow = target.GetComponent<SlowEffectComponent>();

            // 状态叠加或刷新（如果目标身上已经有减速状态，只刷新持续时间）
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
        
        // 归还查询列表，保持 0 GC
        ReturnListToPool(hitEvents);
    }
}