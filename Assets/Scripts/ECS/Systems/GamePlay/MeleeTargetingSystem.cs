// 路径: Assets/Scripts/ECS/Systems/Combat/MeleeTargetingSystem.cs
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
                melee.HasDoubleHit = modifiers.GetLevel("Melee_DoubleHit") > 0;
                melee.Defense = 10f + modifiers.GetLevel("Melee_Armor") * 5f; 
                melee.ThornDamage = melee.Defense * 0.5f; 
                melee.HealthRegen = 5f + modifiers.GetLevel("Melee_Regen") * 2f; 
                melee.LifeStealRatio = 0.1f + modifiers.GetLevel("Melee_LifeSteal") * 0.05f; 
                melee.AttackAngle = Mathf.Min(360f, 90f + modifiers.GetLevel("Melee_RangeEnhance") * 45f);
                melee.AttackRadius = 3f + modifiers.GetLevel("Melee_RangeEnhance") * 0.5f;
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
                var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y, (int)melee.AttackRadius);
                if (nearby.Count > 0)
                {
                    // 发现敌人，抛出挥砍意图
                    p.AddComponent(new MeleeSwingIntentComponent());
                    weapon.CurrentCooldown = weapon.FireRate;
                }
            }
        }
    }
}