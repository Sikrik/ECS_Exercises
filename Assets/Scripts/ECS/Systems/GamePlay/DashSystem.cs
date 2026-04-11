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
        ReturnListToPool(abilities);
    }
}

// ==========================================
// 2. 冲刺触发系统 (只负责消费输入、校验条件并赋予状态)
// ==========================================
public class DashActivationSystem : SystemBase
{
    public DashActivationSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var inputs = GetEntitiesWith<DashInputComponent, DashAbilityComponent>();
        
        // 倒序遍历，因为我们要消费/移除单帧输入组件
        for (int i = inputs.Count - 1; i >= 0; i--)
        {
            var e = inputs[i];
            var ability = e.GetComponent<DashAbilityComponent>();

            // 准入判定：CD 完毕 且 当前不在冲刺中 且 没在受击硬直中
            if (ability.CurrentCD <= 0 && !e.HasComponent<DashStateComponent>() && !e.HasComponent<HitRecoveryComponent>())
            {
                Vector2 dashDir = Vector2.right; // 默认方向

                // 优先根据当前的移动输入确定冲刺方向
                if (e.HasComponent<MoveInputComponent>())
                {
                    var move = e.GetComponent<MoveInputComponent>();
                    if (new Vector2(move.X, move.Y).sqrMagnitude > 0.001f)
                        dashDir = new Vector2(move.X, move.Y).normalized;
                }
                // 若无输入，尝试沿用物理速度方向
                else if (e.HasComponent<VelocityComponent>())
                {
                    var vel = e.GetComponent<VelocityComponent>();
                    if (new Vector2(vel.VX, vel.VY).sqrMagnitude > 0.001f)
                        dashDir = new Vector2(vel.VX, vel.VY).normalized;
                }

                // --- 赋予冲刺物理状态 ---
                e.AddComponent(new DashStateComponent {
                    Timer = ability.Duration,
                    DirX = dashDir.x,
                    DirY = dashDir.y
                });

                // --- 赋予无敌帧与残影表现标签 ---
                if (!e.HasComponent<InvincibleComponent>())
                    e.AddComponent(new InvincibleComponent { Duration = ability.Duration });

                if (!e.HasComponent<GhostTrailComponent>())
                    e.AddComponent(new GhostTrailComponent(0.04f));

                // 重置 CD
                ability.CurrentCD = ability.Cooldown;
                Debug.Log($"<color=cyan>[DashActivationSystem]</color> 发起冲刺！方向: {dashDir}");
            }

            // 无论是否成功冲刺，单帧意图都必须被消耗掉
            e.RemoveComponent<DashInputComponent>();
        }
        ReturnListToPool(inputs);
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
        ReturnListToPool(dashing);
    }
}