using System.Collections.Generic;

/// <summary>
/// 纯粹的伤害结算系统
/// 职责：只处理数值扣减与子弹销毁，绝不插手任何物理击退或硬直逻辑
/// 优势：通过 Faction 阵营完美支持未来敌人发射子弹的需求
/// </summary>
public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 获取所有本帧发生的碰撞事件
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source;

            // 安全校验
            if (target == null || !target.IsAlive || source == null || !source.IsAlive) continue;
            
            // ==========================================
            // 【修改】：全局无敌帧保护（移除玩家独占限制）
            // 只要有无敌组件，任何人都可以免疫伤害
            // ==========================================
            if (target.HasComponent<InvincibleComponent>())
            {
                continue; 
            }

            // 利用阵营 (Faction) 拦截友军伤害
            var sourceFac = source.GetComponent<FactionComponent>();
            var targetFac = target.GetComponent<FactionComponent>();

            // 如果碰撞双方都有阵营，并且阵营相同，则豁免伤害！
            if (sourceFac != null && targetFac != null && sourceFac.Value == targetFac.Value)
            {
                continue;
            }

            // 纯粹的扣血与销毁逻辑
            if (target.HasComponent<HealthComponent>() && source.HasComponent<DamageComponent>())
            {
                var hp = target.GetComponent<HealthComponent>();
                var dmg = source.GetComponent<DamageComponent>();
    
                // 1. 扣除血量
                hp.CurrentHealth -= dmg.Value;

                // 2. 派发受伤事件 (供 EnemyHitReactionSystem 或 PlayerHitReactionSystem 监听)
                if (!target.HasComponent<DamageTakenEventComponent>())
                {
                    target.AddComponent(EventPool.GetDamageEvent(dmg.Value));
                }

                // 3. 如果攻击源是子弹，命中后打上销毁标签
                if (source.HasComponent<BulletTag>() && !source.HasComponent<PendingDestroyComponent>())
                {
                    source.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        
        // 维持 0 GC
        ReturnListToPool(hitEvents);
    }
}