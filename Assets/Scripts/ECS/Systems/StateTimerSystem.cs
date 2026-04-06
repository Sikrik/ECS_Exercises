using System.Collections.Generic;
using UnityEngine;

public class StateTimerSystem : SystemBase
{
    public StateTimerSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 处理击退计时与速度衰减
        var knockbacks = GetEntitiesWith<KnockbackComponent, VelocityComponent>();
        foreach (var e in knockbacks)
        {
            var kb = e.GetComponent<KnockbackComponent>();
            var vel = e.GetComponent<VelocityComponent>();
            
            kb.Timer -= deltaTime;
            float progress = Mathf.Max(0, kb.Timer / 0.2f); // 击退时间硬编码或读配置
            vel.VX = kb.DirX * kb.Speed * progress;
            vel.VY = kb.DirY * kb.Speed * progress;

            if (kb.Timer <= 0) e.RemoveComponent<KnockbackComponent>();
        }

        // 2. 处理硬直计时
        var recoveries = GetEntitiesWith<HitRecoveryComponent>();
        foreach (var e in recoveries)
        {
            var hr = e.GetComponent<HitRecoveryComponent>();
            hr.Timer -= deltaTime;
            if (hr.Timer <= 0) e.RemoveComponent<HitRecoveryComponent>();
        }
        
        // 3. 处理无敌计时 (原本在 InvincibleVisualSystem 里，现在可以移到这里)
    }
}