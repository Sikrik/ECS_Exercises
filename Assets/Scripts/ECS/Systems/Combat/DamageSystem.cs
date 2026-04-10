using System.Collections.Generic;

/// <summary>
/// 纯粹的伤害结算系统
/// 职责：只处理数值扣减与子弹销毁，绝不插手任何物理击退或硬直逻辑
/// </summary>
public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source;

            if (target == null || !target.IsAlive || source == null || !source.IsAlive) continue;
            
            // 玩家无敌帧保护
            if (target.HasComponent<PlayerTag>() && target.HasComponent<InvincibleComponent>())
            {
                continue; 
            }

            // 【拦截同阵营伤害】：防止怪物互相走位时把自己人挤死
            if (source.HasComponent<EnemyTag>() && target.HasComponent<EnemyTag>())
            {
                continue;
            }

            // 纯粹的扣血逻辑
            if (target.HasComponent<HealthComponent>() && source.HasComponent<DamageComponent>())
            {
                var hp = target.GetComponent<HealthComponent>();
                var dmg = source.GetComponent<DamageComponent>();
    
                // 1. 扣除血量
                hp.CurrentHealth -= dmg.Value;

                // 【修复点】：派发受伤事件！这里使用了你写好的 EventPool 对象池实现 0 GC
                if (!target.HasComponent<DamageTakenEventComponent>())
                {
                    target.AddComponent(EventPool.GetDamageEvent(dmg.Value));
                }

                // 2. 子弹命中后，打上销毁标签
                if (source.HasComponent<BulletTag>() && !source.HasComponent<PendingDestroyComponent>())
                {
                    source.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        ReturnListToPool(hitEvents);
    }
}