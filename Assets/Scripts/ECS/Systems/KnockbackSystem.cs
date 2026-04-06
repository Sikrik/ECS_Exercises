using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 击退系统：处理防重叠修正与物理反馈。
/// </summary>
public class KnockbackSystem : SystemBase
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        var hitEntities = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEntities)
        {
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            if (target != null && target.IsAlive)
            {
                var tPos = target.GetComponent<PositionComponent>();

                // --- 核心修复 2：通用位置修正 (防重叠) ---
                // 无论是否有弹性，都必须沿法线推开，防止嵌入重叠
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // --- 差异化反馈：只有有弹性的怪才会被弹飞 ---
                if (target.HasComponent<BouncyTag>())
                {
                    var tVel = target.GetComponent<VelocityComponent>();
                    if (tVel != null)
                    {
                        tVel.VX = evt.Normal.x * config.CollisionBounceForce;
                        tVel.VY = evt.Normal.y * config.CollisionBounceForce;
                    }
                    // 挂载受击硬直，暂时屏蔽 AI 追踪
                    target.AddComponent(new HitRecoveryComponent { Timer = config.EnemyHitRecoveryDuration });
                }
            }
        }
    }
}