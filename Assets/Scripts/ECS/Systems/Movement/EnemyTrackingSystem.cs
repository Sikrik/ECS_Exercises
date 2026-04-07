using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：负责计算怪物的 AI 寻路决策。
/// 【终极解耦版】：不再判断具体的异常状态，完全依赖 StatusSummaryComponent 的汇总结果。
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();

        // 👇 核心改动：查询列表中加入了 StatusSummaryComponent
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent, StatusSummaryComponent>();

        // 推荐使用倒序遍历，更安全
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            var summary = enemy.GetComponent<StatusSummaryComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();

            // ==========================================
            // 1. 状态拦截（极度解耦）
            // ==========================================
            // 如果状态管线判定该实体本帧无法移动（比如被硬直、被冰冻、被眩晕）
            if (!summary.CanMove)
            {
                // 如果它没有在被击退（击退系统会强行接管速度），我们就把它的主动速度归零，防止“滑冰”
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                continue; // 丧失行动能力，直接跳过本帧寻路计算
            }

            // ==========================================
            // 2. 正常寻路计算
            // ==========================================
            var ePos = enemy.GetComponent<PositionComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            // 计算朝向玩家的向量
            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // 只有距离大于一定值才移动，防止怪物和玩家重叠时发生剧烈抖动
            if (dist > 0.1f)
            {
                // 👇 核心改动：直接使用管线汇总好的“最终速度倍率”，不再手写减速公式
                float finalSpeed = stats.MoveSpeed * summary.SpeedMultiplier;

                // 归一化并应用最终速度
                vel.VX = (dx / dist) * finalSpeed;
                vel.VY = (dy / dist) * finalSpeed;
            }
            else
            {
                // 靠得太近时主动停下
                vel.VX = 0;
                vel.VY = 0;
            }
        }
    }
}