// 路径: Assets/Scripts/ECS/Systems/Combat/MeleeExecutionSystem.cs
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
            var intent = p.GetComponent<MeleeSwingIntentComponent>();

            ExecuteMeleeSwing(p, melee, pPos, intent);

            p.RemoveComponent<MeleeSwingIntentComponent>();

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
        Vector2 attackDir = Vector2.right; 

        Entity target = FindNearest(pPos, finalRadius);
        if (target != null)
        {
            var tPos = target.GetComponent<PositionComponent>();
            attackDir = (new Vector2(tPos.X, tPos.Y) - currentPos).normalized;
        }

        var targets = ECSManager.Instance.Grid.GetNearbyEnemies(pPos.X, pPos.Y, Mathf.CeilToInt(finalRadius));
        float finalRadiusSqr = finalRadius * finalRadius; // 【优化】缓存半径平方

        foreach (var e in targets)
        {
            if (!e.IsAlive || e.HasComponent<DeadTag>()) continue;

            var ePos = e.GetComponent<PositionComponent>();
            Vector2 toEnemy = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
            
            // 【优化】避免了昂贵的开方运算
            if (toEnemy.sqrMagnitude <= finalRadiusSqr) 
            {
                if (finalAngle >= 360f || Vector2.Angle(attackDir, toEnemy) <= finalAngle * 0.5f)
                {
                    float dmg = 35f; 
                    if (p.HasComponent<WeaponModifierComponent>())
                    {
                        dmg *= p.GetComponent<WeaponModifierComponent>().GlobalDamageMultiplier;
                    }

                    e.AddComponent(new DamageEventComponent { 
                        DamageAmount = dmg, 
                        Source = p, 
                        IsCritical = false 
                    });
                }
            }
        }

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
            float d = (myPos.x - ePos.X) * (myPos.x - ePos.X) + (myPos.y - ePos.Y) * (myPos.y - ePos.Y);
            if (d < minDist) { minDist = d; nearest = e; }
        }
        return nearest;
    }
}