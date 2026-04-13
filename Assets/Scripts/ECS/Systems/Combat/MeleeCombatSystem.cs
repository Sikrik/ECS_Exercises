// 路径: Assets/Scripts/ECS/Systems/Combat/MeleeCombatSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombatSystem : SystemBase
{
    public MeleeCombatSystem(List<Entity> entities) : base(entities) { }

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
            // 0. 动态同步升级配置 (将 UI 升级项转化为真实数值)
            // ==========================================
            if (p.HasComponent<WeaponModifierComponent>())
            {
                var modifiers = p.GetComponent<WeaponModifierComponent>();
                
                // 二重连击开关
                melee.HasDoubleHit = modifiers.GetLevel("Melee_DoubleHit") > 0;
                
                // 防御力：基础 10，每级 +5
                melee.Defense = 10f + modifiers.GetLevel("Melee_Armor") * 5f; 
                
                // 反伤：根据防御力的 50% 转化
                melee.ThornDamage = melee.Defense * 0.5f; 
                
                // 回血：基础 5，每级 +2
                melee.HealthRegen = 5f + modifiers.GetLevel("Melee_Regen") * 2f; 
                
                // 吸血比率：基础 10%，每级 +5%
                melee.LifeStealRatio = 0.1f + modifiers.GetLevel("Melee_LifeSteal") * 0.05f; 
                
                // 挥砍角度：初始 90°，每级 +45°，封顶 360°
                melee.AttackAngle = Mathf.Min(360f, 90f + modifiers.GetLevel("Melee_RangeEnhance") * 45f);
                
                // 挥砍半径：初始 3，每级 +0.5
                melee.AttackRadius = 3f + modifiers.GetLevel("Melee_RangeEnhance") * 0.5f;
            }

            // ==========================================
            // 1. 初始自然回血 (Health Regen)
            // ==========================================
            if (hp.CurrentHealth < hp.MaxHealth)
            {
                hp.CurrentHealth = Mathf.Min(hp.MaxHealth, hp.CurrentHealth + melee.HealthRegen * deltaTime);
            }

            // ==========================================
            // 2. 二重连击延时触发
            // ==========================================
            if (p.HasComponent<MeleeDoubleHitPendingComponent>())
            {
                var pending = p.GetComponent<MeleeDoubleHitPendingComponent>();
                pending.Timer -= deltaTime;
                if (pending.Timer <= 0)
                {
                    // 倒计时结束，触发第二刀
                    p.AddComponent(new MeleeSwingIntentComponent());
                    p.RemoveComponent<MeleeDoubleHitPendingComponent>();
                }
            }

            // ==========================================
            // 3. 自动索敌近战攻击
            // ==========================================
            if (weapon.CurrentCooldown <= 0 && !p.HasComponent<MeleeDoubleHitPendingComponent>())
            {
                var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y, (int)melee.AttackRadius);
                if (nearby.Count > 0)
                {
                    p.AddComponent(new MeleeSwingIntentComponent());
                    weapon.CurrentCooldown = weapon.FireRate;
                }
            }

            // ==========================================
            // 4. 执行挥砍意图
            // ==========================================
            if (p.HasComponent<MeleeSwingIntentComponent>())
            {
                ExecuteMeleeSwing(p, melee, pos);
            }
        }
    }

    private void ExecuteMeleeSwing(Entity p, MeleeCombatComponent melee, PositionComponent pPos)
    {
        var intent = p.GetComponent<MeleeSwingIntentComponent>();
        
        // 冲刺时会传入 RadiusMultiplier=1.5, AngleOverride=360
        float finalRadius = melee.AttackRadius * intent.RadiusMultiplier;
        float finalAngle = intent.AngleOverride >= 0 ? intent.AngleOverride : melee.AttackAngle;

        Vector2 currentPos = new Vector2(pPos.X, pPos.Y);
        Vector2 attackDir = Vector2.right; // 默认朝右

        // 寻找最近敌人决定攻击朝向
        Entity target = FindNearest(pPos, finalRadius);
        if (target != null)
        {
            var tPos = target.GetComponent<PositionComponent>();
            attackDir = (new Vector2(tPos.X, tPos.Y) - currentPos).normalized;
        }

        // 获取范围内所有敌人实现穿透群体攻击
        var targets = ECSManager.Instance.Grid.GetNearbyEnemies(pPos.X, pPos.Y, Mathf.CeilToInt(finalRadius));

        foreach (var e in targets)
        {
            if (!e.IsAlive || e.HasComponent<DeadTag>()) continue;

            var ePos = e.GetComponent<PositionComponent>();
            Vector2 toEnemy = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
            
            if (toEnemy.magnitude <= finalRadius)
            {
                // 角度判定：如果是 360 度则直接命中，否则计算夹角
                if (finalAngle >= 360f || Vector2.Angle(attackDir, toEnemy) <= finalAngle * 0.5f)
                {
                    float dmg = 35f; 
                    if (p.HasComponent<WeaponModifierComponent>())
                    {
                        dmg *= p.GetComponent<WeaponModifierComponent>().GlobalDamageMultiplier;
                    }

                    // 将玩家 (p) 设为 Source，这样 DamageSystem 就能触发全局吸血
                    e.AddComponent(new DamageEventComponent { 
                        DamageAmount = dmg, 
                        Source = p, 
                        IsCritical = false 
                    });
                }
            }
        }

        // 抛出挥砍 VFX 事件 (表现层)
        Entity vfx = ECSManager.Instance.CreateEntity();
        vfx.AddComponent(new VFXSpawnEventComponent { 
            VFXType = "MeleeSlash", 
            Position = new Vector3(pPos.X, pPos.Y, 0),
            EndPosition = new Vector3(pPos.X + attackDir.x * finalRadius, pPos.Y + attackDir.y * finalRadius, 0),
            NumericParam = finalAngle // 【关键修改】：把算好的攻击角度传给表现层系统生成网格
        });

        // 移除当前挥砍意图
        p.RemoveComponent<MeleeSwingIntentComponent>();

        // ==========================================
        // 5. 判断是否触发二重连击
        // (注：冲刺时的环形斩 RadiusMultiplier > 1，不触发二重连击)
        // ==========================================
        if (melee.HasDoubleHit && intent.RadiusMultiplier == 1.0f && !p.HasComponent<MeleeDoubleHitPendingComponent>())
        {
            // 0.15 秒后挥出第二刀
            p.AddComponent(new MeleeDoubleHitPendingComponent(0.15f)); 
        }
    }

    private Entity FindNearest(PositionComponent pPos, float radius)
    {
        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(pPos.X, pPos.Y, Mathf.CeilToInt(radius));
        Entity nearest = null;
        float minDist = float.MaxValue;
        Vector2 myPos = new Vector2(pPos.X, pPos.Y);
        foreach (var e in enemies)
        {
            var ePos = e.GetComponent<PositionComponent>();
            float d = Vector2.Distance(myPos, new Vector2(ePos.X, ePos.Y));
            if (d < minDist) { minDist = d; nearest = e; }
        }
        return nearest;
    }
}