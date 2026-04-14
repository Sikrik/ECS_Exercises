using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        // 【动态配置】：读取全局滑行摩擦力
        float friction = BattleManager.Instance.Config.KnockbackFriction;

        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            var vel = e.GetComponent<VelocityComponent>();
            if (vel != null)
            {
                // 丝滑减速，使用配置表中的摩擦力系数
                vel.VX = Mathf.Lerp(vel.VX, 0, deltaTime * friction);
                vel.VY = Mathf.Lerp(vel.VY, 0, deltaTime * friction);
            }

            // 滑行时间结束，速度归零，转入硬直状态！
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();

                if (vel != null)
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                
                // 完美衔接硬直状态 (HitRecovery)
                if (kb.HitRecoveryAfterwards > 0 && !e.HasComponent<HitRecoveryComponent>())
                {
                    e.AddComponent(new HitRecoveryComponent { Timer = kb.HitRecoveryAfterwards });
                }
            }
        }
    }
}