using System.Collections.Generic;
using UnityEngine;

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

            if (target == null || !target.IsAlive) continue;
            
            // 玩家无敌帧保护
            if (target.HasComponent<PlayerTag>() && target.HasComponent<InvincibleComponent>())
            {
                continue; 
            }

            // 正常的扣血逻辑
            if (target.HasComponent<HealthComponent>() && source.HasComponent<DamageComponent>())
            {
                var hp = target.GetComponent<HealthComponent>();
                var dmg = source.GetComponent<DamageComponent>();
                
                // 1. 直接扣除基础血量
                hp.CurrentHealth -= dmg.Value;

                // ==========================================
                // 2. 赋予第一目标特权：击退 + 硬直
                // ==========================================
                if (source.HasComponent<BulletTag>() && target.HasComponent<EnemyTag>())
                {
                    // 触发硬直事件 (EnemyHitReactionSystem 会因为这个组件施加硬直)
                    ApplyDamageIntent(target, dmg.Value);

                    // 施加瞬间的击退
                    if (!target.HasComponent<KnockbackComponent>())
                    {
                        var sPos = source.GetComponent<PositionComponent>();
                        var tPos = target.GetComponent<PositionComponent>();
                        Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
                        if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                        pushDir.Normalize();

                        var vel = target.GetComponent<VelocityComponent>();
                        if (vel != null) 
                        {
                            // 赋予物理初速度 (子弹的推力)
                            vel.VX += pushDir.x * 6.0f; 
                            vel.VY += pushDir.y * 6.0f;
                        }
                        
                        // 挂载击退状态，剥夺 AI 控制权，0.1秒后由 KnockbackSystem 负责刹车并彻底转入硬直
                        target.AddComponent(new KnockbackComponent { Timer = 0.1f });
                    }
                }

                // 3. 子弹命中后给自己打上销毁标签
                if (source.HasComponent<BulletTag>() && !source.HasComponent<PendingDestroyComponent>())
                {
                    source.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        ReturnListToPool(hitEvents);
    }

    private void ApplyDamageIntent(Entity target, float damage)
    {
        var existing = target.GetComponent<DamageTakenEventComponent>();
        if (existing != null) existing.DamageAmount += damage;
        else target.AddComponent(EventPool.GetDamageEvent(damage));
    }
}