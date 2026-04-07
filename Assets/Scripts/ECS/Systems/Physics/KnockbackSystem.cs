// Assets/Scripts/ECS/Systems/Physics/KnockbackSystem.cs

using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var config = ECSManager.Instance.Config;
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents) 
        {
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;

            if (target != null && target.IsAlive) 
            {
                var tPos = target.GetComponent<PositionComponent>();

                // 1. 位置修正 (防止重叠)
                tPos.X += evt.Normal.x * config.CollisionPushDistance;
                tPos.Y += evt.Normal.y * config.CollisionPushDistance;

                // 2. 仅处理弹性物理反馈，不再挂载 HitRecoveryComponent
                if (target.HasComponent<BouncyTag>()) 
                {
                    var tVel = target.GetComponent<VelocityComponent>();
                    if (tVel != null) 
                    {
                        tVel.VX = evt.Normal.x * config.CollisionBounceForce;
                        tVel.VY = evt.Normal.y * config.CollisionBounceForce;
                    }
                }
            }
        }
        // 注意：此处不再调用 ReturnListToPool，交由 ECSManager 自动处理

        // 3. 处理持续击退逻辑
        var knockbacks = GetEntitiesWith<KnockbackComponent, VelocityComponent>();
        for (int i = knockbacks.Count - 1; i >= 0; i--)
        {
            var kb = knockbacks[i].GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer > 0)
            {
                var vel = knockbacks[i].GetComponent<VelocityComponent>();
                vel.VX = kb.DirX * kb.Speed;
                vel.VY = kb.DirY * kb.Speed;
            }
            else
            {
                knockbacks[i].RemoveComponent<KnockbackComponent>();
            }
        }
    }
}