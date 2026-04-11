using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冲锋怪 AI 系统
/// 职责：判断条件，当下达冲锋决定时，赋予单帧意图组件
/// </summary>
public class ChargerAISystem : SystemBase
{
    public ChargerAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出拥有冲锋AI、坐标以及冲刺能力的实体
        var chargers = GetEntitiesWith<ChargerAIComponent, PositionComponent, DashAbilityComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        foreach (var enemy in chargers)
        {
            // 处于受击硬直或击退中，无法思考冲锋
            if (enemy.HasComponent<HitRecoveryComponent>() || enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            var ability = enemy.GetComponent<DashAbilityComponent>();
            
            // CD 还没好，直接跳过，此时它会受原本的 EnemyTrackingSystem 控制正常走向玩家
            if (ability.CurrentCD > 0) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var chargerAI = enemy.GetComponent<ChargerAIComponent>();

            // 计算与玩家的距离
            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);
            float distance = toPlayer.magnitude;

            // 当进入冲锋警戒范围时
            if (distance <= chargerAI.TriggerDistance)
            {
                // 确保它当前的移动意图绝对精准指向玩家
                if (enemy.HasComponent<MoveInputComponent>())
                {
                    var dir = toPlayer.normalized;
                    var moveInput = enemy.GetComponent<MoveInputComponent>();
                    moveInput.X = dir.x;
                    moveInput.Y = dir.y;
                }

                // 核心逻辑：下达冲锋指令（贴上单帧意图组件）
                if (!enemy.HasComponent<DashInputComponent>())
                {
                    enemy.AddComponent(new DashInputComponent());
                    // 此时右脑（表现层）可以考虑监听这个，播个红眼变色的特效之类
                }
            }
        }
        
        ReturnListToPool(chargers);
    }
}