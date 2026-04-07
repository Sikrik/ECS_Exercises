using System.Collections.Generic;

public class KnockbackSystem : SystemBase {
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) {
        var config = ECSManager.Instance.Config;
        var hitEntities = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEntities) {
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            if (target != null && target.IsAlive) {
                var tPos = target.GetComponent<PositionComponent>();

                // 通用位置修正
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 仅针对有弹性的实体应用物理反馈
                if (target.HasComponent<BouncyTag>()) {
                    var tVel = target.GetComponent<VelocityComponent>();
                    var stats = target.GetComponent<EnemyStatsComponent>(); // 获取数值组件

                    if (tVel != null) {
                        tVel.VX = evt.Normal.x * config.CollisionBounceForce;
                        tVel.VY = evt.Normal.y * config.CollisionBounceForce;
                    }

                    // 修复：从 stats 组件读取硬直时间，而非 config
                    if (stats != null) {
                        target.AddComponent(new HitRecoveryComponent { 
                            Timer = stats.HitRecoveryDuration 
                        });
                    }
                }
            }
        }
        var knockbacks = GetEntitiesWith<KnockbackComponent, VelocityComponent>();
        for (int i = knockbacks.Count - 1; i >= 0; i--)
        {
            var entity = knockbacks[i];
            var kb = entity.GetComponent<KnockbackComponent>();
            
            // 扣减时间
            kb.Timer -= deltaTime;
            
            if (kb.Timer > 0)
            {
                // 应用击退速度（强行覆盖正常速度）
                var vel = entity.GetComponent<VelocityComponent>();
                vel.VX = kb.DirX * kb.Speed;
                vel.VY = kb.DirY * kb.Speed;
            }
            else
            {
                // 击退结束，移除组件。下一帧 StatusGatherSystem 会重新允许它寻路
                entity.RemoveComponent<KnockbackComponent>();
            }
        }
    }
}