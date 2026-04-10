using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞反馈统一结算系统 (基于数据驱动)
/// 职责：读取攻击方的意图 (ImpactFeedback)，决定受击方是否被击退或硬直
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

            // 1. 获取攻击方的“反馈设定”。如果没有这个组件，说明是“方案一：不反弹不硬直”，直接跳过。
            var feedback = source.GetComponent<ImpactFeedbackComponent>();
            if (feedback == null) continue;

            // 2. 【反弹逻辑】 (方案二 & 方案三 的共性)
            if (feedback.CauseBounce && !target.HasComponent<KnockbackComponent>())
            {
                var sPos = source.GetComponent<PositionComponent>();
                var tPos = target.GetComponent<PositionComponent>();
                
                // 计算排斥方向
                Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pushDir.Normalize();

                // 动态力度计算（保留了你原有的基于质量和弹性配置的优秀手感）
                float finalForce = 6.0f; // 默认推力
                if (source.HasComponent<MassComponent>() && target.HasComponent<MassComponent>())
                {
                    float tMass = target.GetComponent<MassComponent>().Value;
                    float sMass = source.GetComponent<MassComponent>().Value;
                    float pushRatio = sMass / (sMass + tMass); 
                    float baseForce = source.HasComponent<BounceForceComponent>() ? source.GetComponent<BounceForceComponent>().Value : 15f;
                    finalForce = baseForce * pushRatio;
                }

                var vel = target.GetComponent<VelocityComponent>();
                if (vel != null)
                {
                    vel.VX += pushDir.x * finalForce;
                    vel.VY += pushDir.y * finalForce;
                }

                // 挂载击退状态，剥夺目标对速度的控制权，0.15秒后由 KnockbackSystem 负责刹车
                target.AddComponent(new KnockbackComponent { Timer = 0.15f });
            }

            // 3. 【硬直逻辑】 (方案三 特有)
            if (feedback.CauseHitRecovery && !target.HasComponent<HitRecoveryComponent>())
            {
                // 读取目标本身的硬直时间配置
                var stats = target.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.2f;

                if (duration > 0)
                {
                    // 给目标挂载硬直组件，触发高频闪烁并打断 AI 寻路
                    target.AddComponent(new HitRecoveryComponent { Timer = duration });
                }
            }
        }
        ReturnListToPool(hitEvents);
    }
}