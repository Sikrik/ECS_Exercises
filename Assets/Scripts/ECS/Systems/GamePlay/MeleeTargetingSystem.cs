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
                
                // 【修复】全面对齐 Upgrade_Config_Melee.csv
                melee.HasDoubleHit = modifiers.GetLevel("Melee_AddCombo") > 0;
                
                // 吸血机制：有基础吸血项才开启，配合强化项提升比例
                int lifestealBase = modifiers.GetLevel("Melee_AddLifesteal") > 0 ? 1 : 0;
                melee.LifeStealRatio = (lifestealBase > 0 ? 0.05f : 0f) + modifiers.GetLevel("Melee_LifestealEnhance") * 0.02f; 
                
                // 攻击范围与角度
                melee.AttackAngle = Mathf.Min(360f, 90f + modifiers.GetLevel("Melee_IncreaseRadius") * 45f);
                melee.AttackRadius = 3f + modifiers.GetLevel("Melee_IncreaseRadius") * 0.5f;

                // 狂风剑法：攻速提升 (通过降低 Weapon 的 FireRate 实现)
                float speedBonus = modifiers.GetLevel("Melee_AttackSpeed") * 0.1f; // 每级减少 10% 攻击间隔
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
                    // 计时结束，抛出第二击意图
                    p.AddComponent(new MeleeSwingIntentComponent());
                    p.RemoveComponent<MeleeDoubleHitPendingComponent>();
                }
            }
            else if (weapon.CurrentCooldown <= 0)
            {
                // 普通索敌
                var nearby = ECSManager.Instance.Grid.GetNearbyEntities(pos.X, pos.Y, (int)melee.AttackRadius);
                if (nearby.Count > 0)
                {
                    // 发现敌人，抛出挥砍意图
                    p.AddComponent(new MeleeSwingIntentComponent());
                    // 进入冷却
                    weapon.CurrentCooldown = weapon.FireRate;

                    // 如果拥有连击技能，预约下一次挥砍
                    if (melee.HasDoubleHit)
                    {
                        p.AddComponent(new MeleeDoubleHitPendingComponent(0.2f)); 
                    }
                }
            }
        }
    }
}