using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        // 1. 保留原本的怪物互挤 (虫群流动) 逻辑，防止怪物完全重叠 ... 
        // (保持原有 hitEvents 互推逻辑不变)

        // ==========================================
        // 2. 处理击退滑行的平滑减速与【状态衔接】
        // ==========================================
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            var vel = e.GetComponent<VelocityComponent>();
            if (vel != null)
            {
                // 丝滑减速，15f 是摩擦力系数
                vel.VX = Mathf.Lerp(vel.VX, 0, deltaTime * 15f);
                vel.VY = Mathf.Lerp(vel.VY, 0, deltaTime * 15f);
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