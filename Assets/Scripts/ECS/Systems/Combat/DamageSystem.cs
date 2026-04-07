using System.Collections.Generic;

/// <summary>
/// 伤害计算系统：只负责扣血与抛出受伤事件
/// </summary>
public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var attackers = GetEntitiesWith<CollisionEventComponent, DamageComponent>();

        for (int i = attackers.Count - 1; i >= 0; i--)
        {
            var attacker = attackers[i];
            
            // 👇 优化：单次查找
            var evt = attacker.GetComponent<CollisionEventComponent>();
            var dmg = attacker.GetComponent<DamageComponent>();

            if (evt.Target != null && evt.Target.IsAlive)
            {
                var health = evt.Target.GetComponent<HealthComponent>();
                if (health != null) // 👇 优化：替代 HasComponent<HealthComponent>()
                {
                    var invincible = evt.Target.GetComponent<InvincibleComponent>();
                    if (invincible == null) // 👇 优化：替代 !HasComponent<InvincibleComponent>()
                    {
                        // 1. 扣血
                        health.CurrentHealth -= dmg.Value;

                        // 2. 使用对象池抛出受伤事件，实现 0 GC！
                        evt.Target.AddComponent(EventPool.GetDamageEvent(dmg.Value));
                    }
                }
            }
        }
        
        ReturnListToPool(attackers); // 养成好习惯，用完把 List 还给 ECSManager
    }
}