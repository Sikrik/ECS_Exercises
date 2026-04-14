// 路径: Assets/Scripts/ECS/Systems/Combat/DOTSystem.cs
using System.Collections.Generic;
using UnityEngine;


// ==========================================
// 持续伤害处理系统
// ==========================================
public class DOTSystem : SystemBase
{
    public DOTSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<DOTEffectComponent, HealthComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            var dot = e.GetComponent<DOTEffectComponent>();

            dot.Duration -= deltaTime;
            dot.TickTimer -= deltaTime;

            // 每 0.5 秒跳一次伤害
            if (dot.TickTimer <= 0)
            {
                dot.TickTimer = 0.5f;
                float tickDamage = dot.DamagePerSecond * 0.5f;

                // 抛出标准的伤害事件供 DamageSystem 处理，且 DOT 伤害不计为暴击
                if (!e.HasComponent<DamageEventComponent>())
                {
                    e.AddComponent(new DamageEventComponent { 
                        DamageAmount = tickDamage, 
                        IsCritical = false 
                    });
                }
                else
                {
                    e.GetComponent<DamageEventComponent>().DamageAmount += tickDamage;
                }
            }

            // DOT 结束处理
            if (dot.Duration <= 0)
            {
                e.RemoveComponent<DOTEffectComponent>();
                // 通知表现层销毁挂载的特效
                if (e.HasComponent<AttachedVFXComponent>()) 
                {
                    e.AddComponent(new PendingVFXDestroyTag());
                }
            }
        }
    }
}