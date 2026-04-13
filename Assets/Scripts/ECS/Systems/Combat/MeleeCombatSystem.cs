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

            // 1. 处理初始回血
            if (hp.CurrentHealth < hp.MaxHealth)
                hp.CurrentHealth = Mathf.Min(hp.MaxHealth, hp.CurrentHealth + melee.HealthRegen * deltaTime);

            // 2. 自动近战攻击索敌
            if (weapon.CurrentCooldown <= 0)
            {
                var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y, (int)melee.AttackRadius);
                if (nearby.Count > 0)
                {
                    p.AddComponent(new MeleeSwingIntentComponent());
                    weapon.CurrentCooldown = weapon.FireRate;
                }
            }

            // 3. 挥砍结算
            if (p.HasComponent<MeleeSwingIntentComponent>())
                ExecuteMeleeSwing(p, melee, hp, pos);
        }
    }

    private void ExecuteMeleeSwing(Entity p, MeleeCombatComponent melee, HealthComponent pHp, PositionComponent pPos)
    {
        var intent = p.GetComponent<MeleeSwingIntentComponent>();
        float finalRadius = melee.AttackRadius * intent.RadiusMultiplier;
        float finalAngle = intent.AngleOverride >= 0 ? intent.AngleOverride : melee.AttackAngle;

        // 【修复点】：正确封装 Vector2 避免 float 转换错误
        Vector2 currentPos = new Vector2(pPos.X, pPos.Y);
        Vector2 attackDir = Vector2.right;

        // 寻找最近敌人确定攻击方向
        Entity target = FindNearest(pPos, finalRadius);
        if (target != null)
        {
            var tPos = target.GetComponent<PositionComponent>();
            attackDir = (new Vector2(tPos.X, tPos.Y) - currentPos).normalized;
        }

        float totalDamage = 0;
        var targets = ECSManager.Instance.Grid.GetNearbyEnemies(pPos.X, pPos.Y, Mathf.CeilToInt(finalRadius));

        foreach (var e in targets)
        {
            if (!e.IsAlive || e.HasComponent<DeadTag>()) continue;

            var ePos = e.GetComponent<PositionComponent>();
            Vector2 toEnemy = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
            
            if (toEnemy.magnitude <= finalRadius)
            {
                if (Vector2.Angle(attackDir, toEnemy) <= finalAngle * 0.5f)
                {
                    float dmg = 25f; // 基础近战伤害
                    e.GetComponent<HealthComponent>().CurrentHealth -= dmg;
                    totalDamage += dmg;
                    e.AddComponent(EventPool.GetDamageEvent(dmg, true)); // 造成硬直
                }
            }
        }

        // 4. 吸血逻辑
        if (totalDamage > 0)
            pHp.CurrentHealth = Mathf.Min(pHp.MaxHealth, pHp.CurrentHealth + (totalDamage * melee.LifeStealRatio));

        // 5. 抛出 VFX 事件
        Entity vfx = ECSManager.Instance.CreateEntity();
        vfx.AddComponent(new VFXSpawnEventComponent { VFXType = "MeleeSlash", Position = new Vector3(pPos.X, pPos.Y, 0) });

        p.RemoveComponent<MeleeSwingIntentComponent>();
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