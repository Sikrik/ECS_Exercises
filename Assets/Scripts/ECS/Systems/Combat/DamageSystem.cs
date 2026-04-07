using System.Collections.Generic;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var attackers = GetEntitiesWith<CollisionEventComponent, DamageComponent>();

        for (int i = attackers.Count - 1; i >= 0; i--)
        {
            var attacker = attackers[i];
            var evt = attacker.GetComponent<CollisionEventComponent>();
            var dmg = attacker.GetComponent<DamageComponent>();

            if (evt.Target != null && evt.Target.IsAlive)
            {
                // 🚨 核心修复：重新引入 ECS 层面的阵营校验，彻底杜绝友军伤害！
                bool isAttackerBullet = attacker.HasComponent<BulletTag>();
                bool isAttackerEnemy = attacker.HasComponent<EnemyTag>();

                bool isTargetPlayer = evt.Target.HasComponent<PlayerTag>();
                bool isTargetEnemy = evt.Target.HasComponent<EnemyTag>();

                // 规则：只有满足【子弹打敌人】或【敌人打玩家】的条件，才允许造成伤害
                bool canDamage = (isAttackerBullet && isTargetEnemy) || (isAttackerEnemy && isTargetPlayer);

                if (canDamage)
                {
                    var health = evt.Target.GetComponent<HealthComponent>();
                    if (health != null)
                    {
                        var invincible = evt.Target.GetComponent<InvincibleComponent>();
                        if (invincible == null)
                        {
                            // 1. 扣血
                            health.CurrentHealth -= dmg.Value;

                            // 2. 累加或抛出受伤事件
                            var existingDmgEvt = evt.Target.GetComponent<DamageTakenEventComponent>();
                            if (existingDmgEvt != null)
                            {
                                existingDmgEvt.DamageAmount += dmg.Value;
                            }
                            else
                            {
                                evt.Target.AddComponent(EventPool.GetDamageEvent(dmg.Value));
                            }
                        }
                    }
                }
            }
        }
        
        ReturnListToPool(attackers); 
    }
}