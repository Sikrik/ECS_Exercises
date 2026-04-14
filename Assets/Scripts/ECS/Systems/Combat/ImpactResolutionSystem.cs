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
            var attacker = evt.Source;
            var victim = evt.Target;

            if (attacker == null || !attacker.IsAlive || victim == null || !victim.IsAlive) 
                continue;

            // 检查攻击方是否有碰撞反馈意图，且允许触发物理反弹
            if (attacker.HasComponent<ImpactFeedbackComponent>() && 
                attacker.GetComponent<ImpactFeedbackComponent>().CauseBounce)
            {
                float bounceForce = 5f;
                if (attacker.HasComponent<BounceForceComponent>())
                {
                    bounceForce = attacker.GetComponent<BounceForceComponent>().Value;
                }

                // 给受击方施加击退状态
                if (!victim.HasComponent<KnockbackComponent>())
                {
                    victim.AddComponent(new KnockbackComponent { 
                        DirX = evt.Normal.x, 
                        DirY = evt.Normal.y, 
                        Speed = bounceForce, 
                        Timer = 0.15f,
                        HitRecoveryAfterwards = 0f 
                    });
                }
            }
        }
    }
}