using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        // 筛选出所有发生了碰撞的实体
        var hitEntities = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEntities)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();

            // 如果被撞的目标实体具有“弹性”标记（BouncyTag）
            if (evt.Target != null && evt.Target.IsAlive && evt.Target.HasComponent<BouncyTag>())
            {
                var tPos = evt.Target.GetComponent<PositionComponent>();
                var tVel = evt.Target.GetComponent<VelocityComponent>();

                // 1. 位置修正：根据法线将目标推开，防止重叠嵌入
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 2. 速度反弹：赋予目标反方向的冲力
                if (tVel != null)
                {
                    tVel.VX = evt.Normal.x * config.CollisionBounceForce;
                    tVel.VY = evt.Normal.y * config.CollisionBounceForce;
                }

                // 3. 状态挂载：给目标添加受击硬直计时
                evt.Target.AddComponent(new HitRecoveryComponent 
                { 
                    Timer = config.EnemyHitRecoveryDuration 
                });
            }
        }
    }
}