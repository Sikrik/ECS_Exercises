using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理运动维持系统
/// 职责：处理击退的平滑减速(Lerp)，以及怪物之间的软碰撞挤压（虫群流动）
/// </summary>
public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        // ==========================================
        // 1. 怪物互挤 (虫群软碰撞流动) 
        // ==========================================
        // 这部分不是主动攻击，而是物理推挤，所以不需要 ImpactFeedbackComponent
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();
        foreach (var entity in hitEvents) 
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var eA = evt.Source;
            var eB = evt.Target;

            if (eA == null || !eA.IsAlive || eB == null || !eB.IsAlive) continue;

            // 只有怪物和怪物之间才触发虫群排斥流动
            if (eA.HasComponent<EnemyTag>() && eB.HasComponent<EnemyTag>())
            {
                var aPos = eA.GetComponent<PositionComponent>();
                var bPos = eB.GetComponent<PositionComponent>();
                var aVel = eA.GetComponent<VelocityComponent>();
                var bVel = eB.GetComponent<VelocityComponent>();

                Vector2 pushDir = new Vector2(bPos.X - aPos.X, bPos.Y - aPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pushDir.Normalize();

                float swarmPush = 0.03f;
                
                aPos.X -= pushDir.x * swarmPush;
                aPos.Y -= pushDir.y * swarmPush;
                aVel.VX -= pushDir.x * 0.5f;
                aVel.VY -= pushDir.y * 0.5f;

                bPos.X += pushDir.x * swarmPush;
                bPos.Y += pushDir.y * swarmPush;
                bVel.VX += pushDir.x * 0.5f;
                bVel.VY += pushDir.y * 0.5f;
            }
        }
        ReturnListToPool(hitEvents);

        // ==========================================
        // 2. 处理击退滑行的平滑减速(Lerp)与刹车
        // ==========================================
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            var vel = e.GetComponent<VelocityComponent>();
            if (vel != null)
            {
                // 丝滑减速，15f 是摩擦力系数
                vel.VX = Mathf.Lerp(vel.VX, 0, deltaTime * 15f);
                vel.VY = Mathf.Lerp(vel.VY, 0, deltaTime * 15f);
            }

            // 滑行时间结束，稳稳落地
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();

                // 彻底清空残留的极小速度，钉在原地
                if (vel != null)
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                
                // 注意：这里不再需要像以前那样接管 HitRecovery 状态了。
                // 硬直状态已经由 ImpactResolutionSystem 统一接管，职责更加清晰。
            }
        }
        ReturnListToPool(slidingOnes);
    }
}