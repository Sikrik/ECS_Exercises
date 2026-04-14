using System.Collections.Generic;
using UnityEngine;

public class MeleeExecutionSystem : SystemBase
{
    public MeleeExecutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var attackers = GetEntitiesWith<MeleeSwingIntentComponent, MeleeCombatComponent, PositionComponent>();

        for (int i = attackers.Count - 1; i >= 0; i--)
        {
            var p = attackers[i];
            var melee = p.GetComponent<MeleeCombatComponent>();
            var pPos = p.GetComponent<PositionComponent>();
            
            float radius = melee.AttackRadius;
            int executeLvl = p.HasComponent<WeaponModifierComponent>() ? p.GetComponent<WeaponModifierComponent>().GetLevel("Melee_Execute") : 0;

            var targets = ECSManager.Instance.Grid.GetNearbyEntities(pPos.X, pPos.Y, Mathf.CeilToInt(radius));
            foreach (var e in targets)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
                
                float dmg = 35f;
                bool isCrit = false;

                // 斩杀逻辑
                if (executeLvl > 0 && e.HasComponent<HealthComponent>())
                {
                    var hp = e.GetComponent<HealthComponent>();
                    if (hp.CurrentHealth / hp.MaxHealth <= 0.3f) { dmg += 50f * executeLvl; isCrit = true; }
                }

                e.AddComponent(new DamageEventComponent { DamageAmount = dmg, Source = p, IsCritical = isCrit });
            }
            p.RemoveComponent<MeleeSwingIntentComponent>();
        }
    }
}