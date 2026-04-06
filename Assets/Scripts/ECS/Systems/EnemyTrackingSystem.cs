using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：仅处理正常状态下的追踪逻辑，不干扰物理反馈。
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        // 筛选带 EnemyTag 的活着的实体
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent>();

        foreach (var enemy in enemies)
        {
            // --- 核心解耦点 ---
            // 如果敌人身上有“硬直”或“击退”标记，系统直接跳过本帧追踪
            // 这样物理系统的速度反馈（Knockback）就不会被 AI 覆盖掉
            if (enemy.HasComponent<HitRecoveryComponent>() || enemy.HasComponent<KnockbackComponent>())
            {
                continue; 
            }

            var ePos = enemy.GetComponent<PositionComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                float speed = stats.MoveSpeed;
                // 应用减速组件的影响
                if (enemy.HasComponent<SlowEffectComponent>())
                    speed *= (1f - enemy.GetComponent<SlowEffectComponent>().SlowRatio);

                vel.VX = (dx / dist) * speed;
                vel.VY = (dy / dist) * speed;
            }
        }
    }
}