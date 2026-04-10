using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var config = ECSManager.Instance.Config;
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        // 1. 处理碰撞瞬间
        foreach (var entity in hitEvents) 
        {
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            if (target != null && target.IsAlive && target.HasComponent<BouncyTag>()) 
            {
                var tPos = target.GetComponent<PositionComponent>();
                var tVel = target.GetComponent<VelocityComponent>();

                // 位置挤开修正
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 优先读取 BounceForceComponent (需在下文添加定义)
                float force = target.HasComponent<BounceForceComponent>() 
                    ? target.GetComponent<BounceForceComponent>().Value 
                    : config.CollisionBounceForce;

                tVel.VX = evt.Normal.x * force;
                tVel.VY = evt.Normal.y * force;

                // 挂载滑动组件 (计时0.2秒)
                target.AddComponent(new KnockbackComponent { Timer = 0.2f });
            }
        }

        // 2. 处理滑动结束 -> 转硬直
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();
                
                // 停止滑动，进入硬直
                var stats = e.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.5f;
                e.AddComponent(new HitRecoveryComponent { Timer = duration });
                
                // 进入硬直瞬间清空物理速度
                var vel = e.GetComponent<VelocityComponent>();
                if (vel != null) { vel.VX = 0; vel.VY = 0; }
            }
        }
    }
}