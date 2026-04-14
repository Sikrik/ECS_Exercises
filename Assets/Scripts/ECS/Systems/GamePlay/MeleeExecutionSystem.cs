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
            var modifiers = p.HasComponent<WeaponModifierComponent>() ? p.GetComponent<WeaponModifierComponent>() : null;
            
            float radius = melee.AttackRadius;
            int executeLvl = modifiers != null ? modifiers.GetLevel("Melee_Execute") : 0;
            int waveLvl = modifiers != null ? modifiers.GetLevel("Melee_Wave") : 0;

            // 1. 本体范围挥砍判定 (斩杀逻辑)
            var targets = ECSManager.Instance.Grid.GetNearbyEntities(pPos.X, pPos.Y, Mathf.CeilToInt(radius));
            foreach (var e in targets)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
                
                float dmg = 35f * (modifiers != null ? modifiers.GlobalDamageMultiplier : 1f);
                bool isCrit = false;

                // 触发无情斩杀
                if (executeLvl > 0 && e.HasComponent<HealthComponent>())
                {
                    var hp = e.GetComponent<HealthComponent>();
                    if (hp.CurrentHealth / hp.MaxHealth <= 0.3f) 
                    { 
                        dmg += 50f * executeLvl; // 额外真实伤害
                        isCrit = true; 
                    }
                }

                e.AddComponent(new DamageEventComponent { DamageAmount = dmg, Source = p, IsCritical = isCrit });
            }

            // 2. 触发剑气纵横 (发射穿透子弹)
            if (waveLvl > 0 && p.HasComponent<MoveInputComponent>())
            {
                var input = p.GetComponent<MoveInputComponent>();
                Vector2 aimDir = new Vector2(input.X, input.Y);
                if (aimDir == Vector2.zero) aimDir = Vector2.right; // 兜底方向

                // 复用 BulletFactory 生成一个子弹作为剑气
                Entity waveBullet = BulletFactory.Create(BulletType.Normal, new Vector3(pPos.X, pPos.Y, 0), aimDir.normalized, FactionType.Player, modifiers);
                
                if (waveBullet != null)
                {
                    // 强制赋予剑气穿透能力与独立外观
                    waveBullet.AddComponent(new PierceComponent(1 + waveLvl * 2)); 
                    // 这里可以替换成专属的剑气预制体，或者通过挂载特殊颜色组件改变其外观
                    waveBullet.AddComponent(new ColorTintComponent(Color.cyan)); 
                }
            }

            p.RemoveComponent<MeleeSwingIntentComponent>();
        }
    }
}