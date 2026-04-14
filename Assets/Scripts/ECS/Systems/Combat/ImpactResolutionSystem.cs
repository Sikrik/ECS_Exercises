// 路径: Assets/Scripts/ECS/Systems/Combat/ImpactResolutionSystem.cs
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

            // ==========================================
            // 【修复4：补充丢失的基础伤害逻辑！】
            // 将攻击者的基础 DamageComponent 传递给受害者的 DamageEventComponent
            // ==========================================
            if (attacker.HasComponent<DamageComponent>())
            {
                float baseDmg = attacker.GetComponent<DamageComponent>().Value;
                bool isCrit = attacker.HasComponent<CriticalBulletComponent>();

                if (victim.HasComponent<DamageEventComponent>())
                {
                    victim.GetComponent<DamageEventComponent>().DamageAmount += baseDmg;
                }
                else
                {
                    victim.AddComponent(new DamageEventComponent { 
                        DamageAmount = baseDmg, 
                        Source = attacker, 
                        IsCritical = isCrit 
                    });
                }
            }

            // ==========================================
            // 处理碰撞反弹/击退反馈
            // ==========================================
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

                    // 【修复5：真正应用击退的初速度！】
                    // 原本只给了组件，没改 Velocity，导致怪物只是被定身刹车
                    var victimVel = victim.GetComponent<VelocityComponent>();
                    if (victimVel != null)
                    {
                        victimVel.VX = evt.Normal.x * bounceForce;
                        victimVel.VY = evt.Normal.y * bounceForce;
                    }
                }
            }
        }
    }
}