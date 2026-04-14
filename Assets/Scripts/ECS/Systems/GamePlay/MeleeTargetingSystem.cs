// 路径: Assets/Scripts/ECS/Systems/GamePlay/MeleeTargetingSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class MeleeTargetingSystem : SystemBase
{
    public MeleeTargetingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag, MeleeCombatComponent, HealthComponent, WeaponComponent, PositionComponent>();

        foreach (var p in players)
        {
            var melee = p.GetComponent<MeleeCombatComponent>();
            var hp = p.GetComponent<HealthComponent>();
            var weapon = p.GetComponent<WeaponComponent>();
            var pos = p.GetComponent<PositionComponent>();

            // ==========================================
            // 1. 动态同步升级配置 (状态维护)
            // ==========================================
            if (p.HasComponent<WeaponModifierComponent>())
            {
                var modifiers = p.GetComponent<WeaponModifierComponent>();
                
                // 现有的近战能力
                melee.HasDoubleHit = modifiers.GetLevel("Melee_DoubleHit") > 0;
                melee.Defense = 10f + modifiers.GetLevel("Melee_Armor") * 5f; 
                melee.ThornDamage = melee.Defense * 0.5f; 
                melee.HealthRegen = 5f + modifiers.GetLevel("Melee_Regen") * 2f; 
                melee.LifeStealRatio = 0.1f + modifiers.GetLevel("Melee_LifeSteal") * 0.05f; 
                melee.AttackAngle = Mathf.Min(360f, 90f + modifiers.GetLevel("Melee_RangeEnhance") * 45f);
                melee.AttackRadius = 3f + modifiers.GetLevel("Melee_RangeEnhance") * 0.5f;

                // 👇【新增】狂风剑法：攻速提升 (通过降低 Weapon 的 FireRate 实现)
                float speedBonus = modifiers.GetLevel("Melee_AttackSpeed") * 0.1f; // 每级减少 10% 攻击间隔
                // 假设近战基础攻速为 0.8 秒一刀，封顶最快攻速为 0.2 秒一刀
                weapon.FireRate = Mathf.Max(0.2f, 0.8f * (1f - speedBonus));
            }

            // ==========================================
            // 2. 初始自然回血 (状态维护)
            // ==========================================
            if (hp.CurrentHealth < hp.MaxHealth)
            {
                hp.CurrentHealth = Mathf.Min(hp.MaxHealth, hp.CurrentHealth + melee.HealthRegen * deltaTime);
            }

            // ==========================================
            // 3. 连击计时与索敌 (意图生成)
            // ==========================================
            if (p.HasComponent<MeleeDoubleHitPendingComponent>())
            {
                var pending = p.GetComponent<MeleeDoubleHitPendingComponent>();
                pending.Timer -= deltaTime;
                if (pending.Timer <= 0)
                {
                    p.AddComponent(new MeleeSwingIntentComponent());
                    p.RemoveComponent<MeleeDoubleHitPendingComponent>();
                }
            }
            else if (weapon.CurrentCooldown <= 0)
            {
                // 普通索敌
                var nearby = ECSManager.Instance.Grid.GetNearbyEntities(pos.X, pos.Y, Mathf.CeilToInt(melee.AttackRadius));
                bool foundEnemy = false;

                // 【核心修复 2】：确保网格内的实体是敌人，并且在真实攻击距离内
                foreach (var target in nearby)
                {
                    if (target.IsAlive && target.HasComponent<EnemyTag>())
                    {
                        var tPos = target.GetComponent<PositionComponent>();
                        float distSq = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);
                        if (distSq <= melee.AttackRadius * melee.AttackRadius)
                        {
                            foundEnemy = true;
                            break;
                        }
                    }
                }

                if (foundEnemy)
                {
                    p.AddComponent(new MeleeSwingIntentComponent());
                    weapon.CurrentCooldown = weapon.FireRate;
                }
            }
        }
    }
}   