using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲刺逻辑系统
/// 职责：处理冲刺 CD、状态切换、方向锁定及无敌帧赋予
/// </summary>
public class DashSystem : SystemBase
{
    public DashSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 更新所有拥有冲刺能力的 CD
        var abilities = GetEntitiesWith<DashAbilityComponent>();
        foreach (var e in abilities)
        {
            var ability = e.GetComponent<DashAbilityComponent>();
            if (ability.CurrentCD > 0) ability.CurrentCD -= deltaTime;
        }

        // 2. 处理冲刺触发意图
        var inputs = GetEntitiesWith<DashInputComponent, DashAbilityComponent>();
        foreach (var e in inputs)
        {
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
                // 如果没有输入，则尝试根据当前物理速度确定方向
                else if (e.HasComponent<VelocityComponent>())
                {
                    var vel = e.GetComponent<VelocityComponent>();
                    if (new Vector2(vel.VX, vel.VY).sqrMagnitude > 0.001f)
                        dashDir = new Vector2(vel.VX, vel.VY).normalized;
                }

                // --- 进入冲刺状态 ---
                e.AddComponent(new DashStateComponent {
                    Timer = ability.Duration,
                    DirX = dashDir.x,
                    DirY = dashDir.y
                });

                // --- 赋予无敌帧 ---
                if (!e.HasComponent<InvincibleComponent>())
                {
                    e.AddComponent(new InvincibleComponent { Duration = ability.Duration });
                }

                // --- 开启残影效果标记 ---
                if (!e.HasComponent<GhostTrailComponent>())
                {
                    e.AddComponent(new GhostTrailComponent(0.04f));
                }

                // 重置 CD
                ability.CurrentCD = ability.Cooldown;

                // Debug 提示
                Debug.Log($"<color=cyan>[DashSystem]</color> 实体 {e.GetHashCode()} 发起冲刺！方向: {dashDir}, 无敌时间: {ability.Duration}s");
            }
            else if (ability.CurrentCD > 0)
            {
                Debug.Log($"<color=yellow>[DashSystem]</color> 冲刺冷却中... 剩余: {ability.CurrentCD:F1}s");
            }

            // 消耗掉单帧意图组件
            e.RemoveComponent<DashInputComponent>();
        }

        // 3. 处理冲刺状态结束判定
        var dashing = GetEntitiesWith<DashStateComponent>();
        for (int i = dashing.Count - 1; i >= 0; i--)
        {
            var e = dashing[i];
            var state = e.GetComponent<DashStateComponent>();
            
            state.Timer -= deltaTime;
            if (state.Timer <= 0)
            {
                e.RemoveComponent<DashStateComponent>();
                
                // 冲刺结束，移除残影标记
                if (e.HasComponent<GhostTrailComponent>())
                {
                    e.RemoveComponent<GhostTrailComponent>();
                }
                
                Debug.Log($"<color=white>[DashSystem]</color> 实体 {e.GetHashCode()} 冲刺状态结束");
            }
        }
    }
}