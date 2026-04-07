using System.Collections.Generic;
using UnityEngine;

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

            if (evt.Target != null && evt.Target.IsAlive && evt.Target.HasComponent<HealthComponent>())
            {
                if (!evt.Target.HasComponent<InvincibleComponent>())
                {
                    // 1. 纯粹的扣血逻辑
                    var health = evt.Target.GetComponent<HealthComponent>();
                    health.CurrentHealth -= dmg.Value;

                    // 2. 贴上“受伤事件”标签，甩手不管！
                    evt.Target.AddComponent(new DamageTakenEventComponent(dmg.Value));
                }
            }
        }
    }
}