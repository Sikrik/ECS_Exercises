using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var config = ECSManager.Instance.Config;
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        // 1. 触发阶段：处理新的碰撞
        foreach (var entity in hitEvents) 
        {
            // 排除子弹，子弹不触发物理反弹
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            if (target != null && target.IsAlive && target.HasComponent<BouncyTag>()) 
            {
                var tPos = target.GetComponent<PositionComponent>();
                var tVel = target.GetComponent<VelocityComponent>();

                // 瞬间位置修正（防重叠）
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 计算反弹速度
                float force = target.HasComponent<BounceForceComponent>() 
                    ? target.GetComponent<BounceForceComponent>().Value 
                    : config.CollisionBounceForce;

                tVel.VX = evt.Normal.x * force;
                tVel.VY = evt.Normal.y * force;

                // 挂上击退标签，赋予生存时间（例如滑动0.2秒）
                // 只要有这个组件，EnemyTrackingSystem 就会自动停止寻路
                target.AddComponent(new KnockbackComponent { Timer = 0.2f });
            }
        }

        // 2. 状态流转阶段：处理击退结束转入硬直
        var activeKBs = GetEntitiesWith<KnockbackComponent>();
        for (int i = activeKBs.Count - 1; i >= 0; i--)
        {
            var e = activeKBs[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();
                
                // 击退滑动结束，立即挂上硬直标签
                var stats = e.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.5f;
                e.AddComponent(new HitRecoveryComponent { Timer = duration });
                
                // 硬直开始时速度清零（防止滑动惯性带入硬直）
                var vel = e.GetComponent<VelocityComponent>();
                if (vel != null) { vel.VX = 0; vel.VY = 0; }
            }
        }
    }
}