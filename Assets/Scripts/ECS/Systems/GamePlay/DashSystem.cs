using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 1. 冲刺冷却系统 (纯粹的 CD 扣减)
// ==========================================
public class DashCooldownSystem : SystemBase
{
    public DashCooldownSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var abilities = GetEntitiesWith<DashAbilityComponent>();
        foreach (var e in abilities)
        {
            var ability = e.GetComponent<DashAbilityComponent>();
            if (ability.CurrentCD > 0) 
            {
                ability.CurrentCD -= deltaTime;
            }
        }

    }
}

// ==========================================
// 2. 冲刺触发系统 (只负责消费输入、校验条件并赋予状态)
// ==========================================
// 路径: Assets/Scripts/ECS/Systems/GamePlay/DashSystem.cs

public class DashActivationSystem : SystemBase
{
    public DashActivationSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var inputs = GetEntitiesWith<DashInputComponent, DashAbilityComponent>();
        
        for (int i = inputs.Count - 1; i >= 0; i--)
        {
            var e = inputs[i];
            var ability = e.GetComponent<DashAbilityComponent>();

            if (ability.CurrentCD <= 0 && !e.HasComponent<DashStateComponent>())
            {
                // ... 现有方向计算逻辑 ...

                // --- 赋予物理状态与无敌 ---
                e.AddComponent(new DashStateComponent { /* ... */ });
                e.AddComponent(new InvincibleComponent { Duration = ability.Duration });

                // ==========================================
                // 【新增】近战职业冲刺环形斩
                // ==========================================
                if (e.HasComponent<MeleeCombatComponent>())
                {
                    // 抛出 360 度，1.5 倍半径的挥砍意图
                    e.AddComponent(new MeleeSwingIntentComponent { 
                        RadiusMultiplier = 1.5f, 
                        AngleOverride = 360f 
                    });
                }

                ability.CurrentCD = ability.Cooldown;
            }
            e.RemoveComponent<DashInputComponent>();
        }
    }
}

// ==========================================
// 3. 冲刺状态系统 (只负责计算冲刺时间并执行结束清理)
// ==========================================
public class DashStateSystem : SystemBase
{
    public DashStateSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var dashing = GetEntitiesWith<DashStateComponent>();
        
        for (int i = dashing.Count - 1; i >= 0; i--)
        {
            var e = dashing[i];
            var state = e.GetComponent<DashStateComponent>();
            
            // 扣减状态持续时间
            state.Timer -= deltaTime;
            if (state.Timer <= 0)
            {
                // 冲刺结束，剥离物理状态组件
                e.RemoveComponent<DashStateComponent>();
                
                // 剥离残影标签 (无敌组件有自己的 InvincibleVisualSystem 去处理生命周期，这里不管)
                if (e.HasComponent<GhostTrailComponent>())
                {
                    e.RemoveComponent<GhostTrailComponent>();
                }
                
                Debug.Log($"<color=white>[DashStateSystem]</color> 冲刺状态结束");
            }
        }

    }
}