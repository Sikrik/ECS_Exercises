// 路径: Assets/Scripts/ECS/Systems/Combat/ImpactResolutionSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞解析系统 (Data-Driven 高内聚重构版)
/// </summary>
public class ImpactResolutionSystem : SystemBase
{
    public ImpactResolutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var source = evt.Source; 
            var target = evt.Target; 

            if (source == null || !source.IsAlive || target == null || !target.IsAlive) continue;

            bool sourceIsPhysical = source.HasComponent<MassComponent>();
            bool targetIsPhysical = target.HasComponent<MassComponent>();

            if (!sourceIsPhysical || !targetIsPhysical) 
            {
                continue;
            }

            var fSource = source.GetComponent<FactionComponent>();
            var fTarget = target.GetComponent<FactionComponent>();
            
            var posS = source.GetComponent<PositionComponent>();
            var posT = target.GetComponent<PositionComponent>();

            Vector2 dirToTarget = new Vector2(posT.X - posS.X, posT.Y - posS.Y);
            
            if (dirToTarget.sqrMagnitude < 0.001f) 
            {
                dirToTarget = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                // 👇 【修复 7】增加极值检测，防止 Random 同时随出 (0,0) 导致的 Normalize 抛出 NaN
                if (dirToTarget.sqrMagnitude < 0.001f) dirToTarget = Vector2.right; 
            }
            dirToTarget.Normalize();

            // 同阵营碰撞（软排斥）
            if (fSource != null && fTarget != null && fSource.Value == fTarget.Value)
            {
                float slideSpeed = 2.0f * deltaTime; 
                posS.X -= dirToTarget.x * slideSpeed;
                posS.Y -= dirToTarget.y * slideSpeed;
                posT.X += dirToTarget.x * slideSpeed;
                posT.Y += dirToTarget.y * slideSpeed;
                continue;
            }

            // 敌对阵营碰撞（物理弹开）
            var fbSource = source.GetComponent<ImpactFeedbackComponent>();
            var fbTarget = target.GetComponent<ImpactFeedbackComponent>();

            if (fbSource != null && fbSource.CauseBounce)
            {
                ApplyBounce(target, dirToTarget);
            }

            if (fbTarget != null && fbTarget.CauseBounce)
            {
                ApplyBounce(source, -dirToTarget);
            }
        }
    }

    private void ApplyBounce(Entity victim, Vector2 pushDirection)
    {
        var vVel = victim.GetComponent<VelocityComponent>();
        if (vVel != null)
        {
            float force = victim.HasComponent<BounceForceComponent>() 
                ? victim.GetComponent<BounceForceComponent>().Value 
                : 12f; 
            
            vVel.VX = pushDirection.x * force;
            vVel.VY = pushDirection.y * force;
        }

        if (!victim.HasComponent<KnockbackComponent>())
        {
            float recovery = victim.HasComponent<HitRecoveryStatsComponent>() 
                ? victim.GetComponent<HitRecoveryStatsComponent>().Duration 
                : 0.1f;
            
            victim.AddComponent(new KnockbackComponent { Timer = 0.15f, HitRecoveryAfterwards = recovery });
        }
    }
}