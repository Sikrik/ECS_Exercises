using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 击退系统：处理实体间的物理排斥效果，排除子弹触发的击退。
/// </summary>
public class KnockbackSystem : SystemBase
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        
        // 筛选出所有发生了碰撞事件的实体
        var hitEntities = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEntities)
        {
            // 核心修复：如果发起碰撞的是子弹，则不产生物理弹开效果
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            // 仅对具有 BouncyTag（弹性标记）的活着的实体生效
            if (target != null && target.IsAlive && target.HasComponent<BouncyTag>())
            {
                var tPos = target.GetComponent<PositionComponent>();
                var tVel = target.GetComponent<VelocityComponent>();

                // 1. 位置修正：根据碰撞法线将目标推开，防止重叠嵌入
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 2. 速度反馈：赋予目标瞬间的反向冲力
                if (tVel != null)
                {
                    tVel.VX = evt.Normal.x * config.CollisionBounceForce;
                    tVel.VY = evt.Normal.y * config.CollisionBounceForce;
                }

                // 3. 状态挂载：给目标添加受击硬直（EnemyAISystem 会根据此组件暂停追踪）
                target.AddComponent(new HitRecoveryComponent 
                { 
                    Timer = config.EnemyHitRecoveryDuration 
                });
            }
        }
    }
}