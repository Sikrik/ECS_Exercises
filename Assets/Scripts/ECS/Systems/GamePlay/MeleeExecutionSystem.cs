// 路径: Assets/Scripts/ECS/Systems/Combat/MeleeExecutionSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class MeleeExecutionSystem : SystemBase
{
    public MeleeExecutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 任何拥有 MeleeSwingIntent 的实体都可以触发挥砍（不再局限于 PlayerTag）
        var attackers = GetEntitiesWith<MeleeSwingIntentComponent, MeleeCombatComponent, PositionComponent>();

        // 倒序遍历防止移除组件时影响迭代
        for (int i = attackers.Count - 1; i >= 0; i--)
        {
            var p = attackers[i];
            var melee = p.GetComponent<MeleeCombatComponent>();
            var pPos = p.GetComponent<PositionComponent>();
            var intent = p.GetComponent<MeleeSwingIntentComponent>();

            ExecuteMeleeSwing(p, melee, pPos, intent);

            // 消费意图：一帧只执行一次，执行完销毁
            p.RemoveComponent<MeleeSwingIntentComponent>();

            // 判断是否触发二重连击
            if (melee.HasDoubleHit && intent.RadiusMultiplier == 1.0f && !p.HasComponent<MeleeDoubleHitPendingComponent>())
            {
                p.AddComponent(new MeleeDoubleHitPendingComponent(0.15f)); 
            }
        }
    }

    private void ExecuteMeleeSwing(Entity p, MeleeCombatComponent melee, PositionComponent pPos, MeleeSwingIntentComponent intent)
    {
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
                if (finalAngle >= 360f || Vector2.Angle(attackDir, toEnemy) <= finalAngle * 0.5f)
                {
                    float dmg = 35f; 
                    if (p.HasComponent<WeaponModifierComponent>())
                    {
                        dmg *= p.GetComponent<WeaponModifierComponent>().GlobalDamageMultiplier;
                    }

                    // 抛出标准的 DamageEvent 意图，让刚才重构过的 DamageSystem 去统一结算护甲和吸血
                    e.AddComponent(new DamageEventComponent { 
                        DamageAmount = dmg, 
                        Source = p, 
                        IsCritical = false 
                    });
                }
            }
        }

        // 抛出挥砍 VFX 事件和 Audio 事件
        Entity vfx = ECSManager.Instance.CreateEntity();
        vfx.AddComponent(new VFXSpawnEventComponent { 
            VFXType = "MeleeSlash", 
            Position = new Vector3(pPos.X, pPos.Y, 0),
            EndPosition = new Vector3(pPos.X + attackDir.x * finalRadius, pPos.Y + attackDir.y * finalRadius, 0),
            NumericParam = finalAngle 
        });
        
        Entity audioEvent = ECSManager.Instance.CreateEntity();
        audioEvent.AddComponent(new AudioPlayEventComponent(
            "MeleeSwing", 
            true,         
            new Vector3(pPos.X, pPos.Y, 0)
        ));
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
            float d = (myPos.x - ePos.X) * (myPos.x - ePos.X) + (myPos.y - ePos.Y) * (myPos.y - ePos.Y); // 用 sqrMagnitude 优化性能
            if (d < minDist) { minDist = d; nearest = e; }
        }
        return nearest;
    }
}