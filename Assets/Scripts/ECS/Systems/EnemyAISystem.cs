using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人AI系统：负责根据实体的状态组件决定移动和攻击行为
/// 重构要点：
/// 1. 状态分离：击退、硬直和正常追踪逻辑完全解耦。
/// 2. 组件驱动：通过判断是否拥有状态组件来切换 AI 逻辑，而不是通过数值判断。
/// </summary>
public class EnemyAISystem : SystemBase
{
    public EnemyAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (Time.timeScale <= 0) return;

        // 1. 获取玩家位置（用于追踪）
        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count == 0) return;
        var pPos = players[0].GetComponent<PositionComponent>();

        // 2. 筛选所有需要执行 AI 的敌人
        // 核心过滤器：EnemyTag (身份), PositionComponent (位置), VelocityComponent (控制移动), EnemyStatsComponent (基础属性)
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eVel = enemy.GetComponent<VelocityComponent>();
            var eStats = enemy.GetComponent<EnemyStatsComponent>();

            // --- 优先级逻辑 1：处理击退 (Knockback) ---
            if (enemy.HasComponent<KnockbackComponent>())
            {
                UpdateKnockbackState(enemy, eVel, deltaTime);
                continue; // 击退期间跳过正常追踪
            }

            // --- 优先级逻辑 2：处理受击硬直 (HitRecovery) ---
            if (enemy.HasComponent<HitRecoveryComponent>())
            {
                UpdateRecoveryState(enemy, eVel, ePos, pPos, eStats, deltaTime);
                continue; // 硬直期间跳过正常追踪
            }

            // --- 优先级逻辑 3：正常追踪玩家 ---
            UpdateTrackingState(eVel, ePos, pPos, eStats);
        }
    }

    /// <summary>
    /// 处理击退逻辑：根据击退组件的数据设置速度，并管理组件生命周期
    /// </summary>
    private void UpdateKnockbackState(Entity entity, VelocityComponent vel, float dt)
    {
        var kb = entity.GetComponent<KnockbackComponent>();
        
        // 速度线性衰减（基于剩余时间）
        float progress = kb.Timer / 0.2f; // 假设击退基准时间为0.2秒，可从配置读取
        vel.X = kb.DirX * kb.Speed * progress;
        vel.Y = kb.DirY * kb.Speed * progress;

        kb.Timer -= dt;
        if (kb.Timer <= 0)
        {
            // 击退结束，移除状态组件
            entity.RemoveComponent<KnockbackComponent>();
        }
    }

    /// <summary>
    /// 处理恢复逻辑：在硬直期间尝试缓慢恢复向玩家移动
    /// </summary>
    private void UpdateRecoveryState(Entity entity, VelocityComponent vel, PositionComponent ePos, PositionComponent pPos, EnemyStatsComponent stats, float dt)
    {
        var recovery = entity.GetComponent<HitRecoveryComponent>();
        
        // 简单处理：硬直期间速度为0，或者像原来代码一样缓慢恢复
        float dx = pPos.X - ePos.X;
        float dy = pPos.Y - ePos.Y;
        float mag = Mathf.Sqrt(dx * dx + dy * dy);

        if (mag > 0.1f)
        {
            // 速度随硬直结束线性恢复
            // 假设硬直总时长为 config.EnemyHitRecoveryDuration
            float progress = 1.0f - (recovery.Timer / 0.4f); 
            vel.X = (dx / mag) * stats.MoveSpeed * progress;
            vel.Y = (dy / mag) * stats.MoveSpeed * progress;
        }

        recovery.Timer -= dt;
        if (recovery.Timer <= 0)
        {
            entity.RemoveComponent<HitRecoveryComponent>();
        }
    }

    /// <summary>
    /// 正常追踪：计算方向并应用全额移动速度
    /// </summary>
    private void UpdateTrackingState(VelocityComponent vel, PositionComponent ePos, PositionComponent pPos, EnemyStatsComponent stats)
    {
        float dx = pPos.X - ePos.X;
        float dy = pPos.Y - ePos.Y;
        float mag = Mathf.Sqrt(dx * dx + dy * dy);

        if (mag > 0.1f)
        {
            vel.X = (dx / mag) * stats.MoveSpeed;
            vel.Y = (dy / mag) * stats.MoveSpeed;
        }
        else
        {
            vel.X = 0;
            vel.Y = 0;
        }
    }
}