// 路径: Assets/Scripts/ECS/Systems/GamePlay/ChargerAISystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲锋怪 AI 系统
/// 职责：判断条件，当下达冲锋决定时，进入蓄力阶段并开启范围预测
/// </summary>
public class ChargerAISystem : SystemBase
{
    public ChargerAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var chargers = GetEntitiesWith<ChargerAIComponent, PositionComponent, DashAbilityComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        foreach (var enemy in chargers)
        {
            // 如果已经在蓄力、冲刺或处于硬直，跳过 AI 决策
            if (enemy.HasComponent<DashPrepStateComponent>() || 
                enemy.HasComponent<DashStateComponent>() ||
                enemy.HasComponent<HitRecoveryComponent>() || 
                enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            var ability = enemy.GetComponent<DashAbilityComponent>();
            if (ability.CurrentCD > 0) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var chargerAI = enemy.GetComponent<ChargerAIComponent>();

            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);
            float distance = toPlayer.magnitude;

            // ==========================================
            // 加入动态触发偏移（-1.5米 到 +1.5米），防止玩家精准背板
            // ==========================================
            float triggerOffset = (Mathf.PerlinNoise(enemy.GetHashCode(), Time.time * 0.5f) * 2f - 1f) * 1.5f;
            float dynamicTriggerDist = chargerAI.TriggerDistance + triggerOffset;

            // 进入蓄力警戒范围
            if (distance <= dynamicTriggerDist)
            {
                Vector2 dashDir = toPlayer.normalized;

                // 动态计算真实的冲刺距离 = 冲刺速度 * 冲刺时间
                float actualDashDistance = ability.DashSpeed * ability.Duration;

                // 1. 赋予蓄力状态组件 (锁定方向，蓄力 0.8 秒)
                enemy.AddComponent(new DashPrepStateComponent(0.8f, dashDir));
                
                // 2. 赋予预览意图组件 (告知表现层：长度使用真实计算距离，宽 1.2 米)
                enemy.AddComponent(new DashPreviewIntentComponent(dashDir, actualDashDistance, 1.2f));

                // 3. 停止当前常规寻路移动
                if (enemy.HasComponent<MoveInputComponent>())
                {
                    var moveInput = enemy.GetComponent<MoveInputComponent>();
                    moveInput.X = 0;
                    moveInput.Y = 0;
                }
            }
        }

    }
}