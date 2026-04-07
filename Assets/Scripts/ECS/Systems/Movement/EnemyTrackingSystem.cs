using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：基于 StatusSummaryComponent 进行寻路决策。
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent, StatusSummaryComponent>();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            var summary = enemy.GetComponent<StatusSummaryComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();

            // 1. 状态拦截
            if (!summary.CanMove)
            {
                // 👇 优化：单次查找替代 HasComponent
                var knockback = enemy.GetComponent<KnockbackComponent>();
                if (knockback == null) 
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                continue; 
            }

            // 2. 正常寻路计算
            var ePos = enemy.GetComponent<PositionComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                float finalSpeed = stats.MoveSpeed * summary.SpeedMultiplier;
                vel.VX = (dx / dist) * finalSpeed;
                vel.VY = (dy / dist) * finalSpeed;
            }
            else
            {
                vel.VX = 0;
                vel.VY = 0;
            }
        }
        
        ReturnListToPool(enemies);
    }
}