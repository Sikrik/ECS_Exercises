using System.Collections.Generic;
using UnityEngine;

public class ImpactResolutionSystem : SystemBase
{
    public ImpactResolutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var source = evt.Source; // 碰撞发起者
            var target = evt.Target; // 被撞者

            if (source == null || !source.IsAlive || target == null || !target.IsAlive) continue;

            // 1. 获取物理配置
            var sPos = source.GetComponent<PositionComponent>();
            var tPos = target.GetComponent<PositionComponent>();
            var sCol = source.GetComponent<CollisionComponent>();
            var tCol = target.GetComponent<CollisionComponent>();

            // ==========================================
            // 核心修复：强制位置分离 (Depenetration)
            // ==========================================
            if (sCol != null && tCol != null)
            {
                Vector2 diff = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
                float dist = diff.magnitude;
                float minDist = sCol.Radius + tCol.Radius;

                // 如果两个圆形的距离小于半径之和，说明发生了穿透/重叠
                if (dist < minDist)
                {
                    float overlapDepth = minDist - dist;
                    Vector2 separationVec = (dist < 0.001f ? Vector2.up : diff / dist) * (overlapDepth + 0.05f);

                    // 玩家通常具有更高的优先级，强制将敌人推开
                    if (source.HasComponent<PlayerTag>())
                    {
                        tPos.X += separationVec.x;
                        tPos.Y += separationVec.y;
                    }
                    else if (target.HasComponent<PlayerTag>())
                    {
                        // 如果 Target 是玩家，则反向推开 Source (敌人)
                        sPos.X -= separationVec.x;
                        sPos.Y -= separationVec.y;
                    }
                    else
                    {
                        // 怪物之间对半推开
                        tPos.X += separationVec.x * 0.5f;
                        tPos.Y += separationVec.y * 0.5f;
                        sPos.X -= separationVec.x * 0.5f;
                        sPos.Y -= separationVec.y * 0.5f;
                    }
                }
            }

            // 2. 获取攻击方的反馈设定
            var feedback = source.GetComponent<ImpactFeedbackComponent>();
            if (feedback == null) continue;

            // 3. 处理弹性反弹逻辑
            if (feedback.CauseBounce && !target.HasComponent<KnockbackComponent>())
            {
                Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = Random.insideUnitCircle.normalized;
                pushDir.Normalize();

                float finalForce = 6.0f; 
                if (source.HasComponent<MassComponent>() && target.HasComponent<MassComponent>())
                {
                    float tMass = target.GetComponent<MassComponent>().Value;
                    float sMass = source.GetComponent<MassComponent>().Value;
                    float pushRatio = sMass / (sMass + tMass); 
                    float baseForce = source.HasComponent<BounceForceComponent>() ? source.GetComponent<BounceForceComponent>().Value : 5f;
                    finalForce = baseForce * pushRatio;
                }

                var vel = target.GetComponent<VelocityComponent>();
                if (vel != null)
                {
                    vel.VX += pushDir.x * finalForce;
                    vel.VY += pushDir.y * finalForce;
                }

                // 挂载击退状态，防止输入立即覆盖物理效果
                target.AddComponent(new KnockbackComponent { Timer = 0.15f });
            }

            // 4. 处理受击硬直逻辑
            if (feedback.CauseHitRecovery && !target.HasComponent<HitRecoveryComponent>())
            {
                var stats = target.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.2f;

                if (duration > 0)
                {
                    target.AddComponent(new HitRecoveryComponent { Timer = duration });
                }
            }
        }
    }
}