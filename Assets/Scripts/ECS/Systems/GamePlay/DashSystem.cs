// 路径: Assets/Scripts/ECS/Systems/GamePlay/DashSystem.cs
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
// 2. 冲刺触发系统 (只负责消费输入、校验条件、赋予状态并抛出事件)
// ==========================================
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

            // 检查冷却完毕 且 当前不在冲刺状态中
            if (ability.CurrentCD <= 0 && !e.HasComponent<DashStateComponent>())
            {
                // ==========================================
                // 1. 计算冲刺方向
                // ==========================================
                Vector2 dashDir = Vector2.zero;

                // 优先判断是否是怪物AI预先算好的方向（比如 Charger 蓄力锁定）
                if (e.HasComponent<DashPrepStateComponent>())
                {
                    dashDir = e.GetComponent<DashPrepStateComponent>().TargetDir;
                }
                // 否则读取玩家的移动输入意图（WASD）
                else if (e.HasComponent<MoveInputComponent>())
                {
                    var move = e.GetComponent<MoveInputComponent>();
                    dashDir = new Vector2(move.X, move.Y).normalized;
                }

                // 兜底保护：如果没有按下方向键，默认朝右冲刺
                if (dashDir == Vector2.zero) dashDir = Vector2.right;

                // ==========================================
                // 2. 赋予物理状态与无敌
                // ==========================================
                e.AddComponent(new DashStateComponent { 
                    DirX = dashDir.x, 
                    DirY = dashDir.y, 
                    Timer = ability.Duration 
                });
                
                // 赋予无敌状态
                e.AddComponent(new InvincibleComponent { Duration = ability.Duration });

                // ==========================================
                // 3. 赋予残影标记，触发 GhostTrailSystem 视觉生成
                // ==========================================
                e.AddComponent(new GhostTrailComponent());

                // ==========================================
                // 4. 【高内聚解耦】：抛出冲刺事件 (0 GC)
                // 不再硬编码判断 MeleeCombatComponent，
                // 而是让其他关心的系统（如 MeleeDashReactionSystem）自己去监听这个事件。
                // ==========================================
                e.AddComponent(EventPool.GetDashStartedEvent());

                // 进入冷却
                ability.CurrentCD = ability.Cooldown;
            }
            
            // 无论本帧是否成功触发冲刺，都必须消费掉（移除）输入意图组件
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
            }
        }
    }
}