using System.Collections.Generic;
using UnityEngine;

public class DOTSystem : SystemBase
{
    public DOTSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<DOTEffectComponent, HealthComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            var dot = e.GetComponent<DOTEffectComponent>();

            dot.Duration -= deltaTime;
            dot.TickTimer -= deltaTime;

            if (dot.TickTimer <= 0)
            {
                dot.TickTimer = 0.5f;
                float tickDamage = dot.DamagePerSecond * 0.5f;

                if (!e.HasComponent<DamageEventComponent>())
                    e.AddComponent(new DamageEventComponent { DamageAmount = tickDamage, IsCritical = false });
                else
                    e.GetComponent<DamageEventComponent>().DamageAmount += tickDamage;
            }

            if (dot.Duration <= 0)
            {
                e.RemoveComponent<DOTEffectComponent>();
                if (e.HasComponent<AttachedVFXComponent>()) e.AddComponent(new PendingVFXDestroyTag());
            }
        }
    }
}