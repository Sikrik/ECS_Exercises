using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：负责计算怪物的 AI 寻路决策。
/// 仅处理正常状态下的追踪逻辑，不干扰硬直与物理击退反馈。
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        // 筛选出所有活着的敌人，并获取它们的坐标、速度和数值组件
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent>();

        foreach (var enemy in enemies)
        {
            // --- 核心解耦点：数据屏障 ---
            // 如果敌人被打出了硬直（懵了）
            if (enemy.HasComponent<HitRecoveryComponent>())
            {
                // 如果它没有在天上飞（被击退），就把它钉在原地
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    var v = enemy.GetComponent<VelocityComponent>();
                    v.VX = 0;
                    v.VY = 0;
                }
                continue; // 大脑宕机，跳过本帧的寻路计算
            }
            
            // 如果敌人没有硬直，但正在被击退，也只跳过寻路，不归零速度（让击退系统控制）
            if (enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            // --- 正常的寻路计算 ---
            var ePos = enemy.GetComponent<PositionComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            // 计算朝向玩家的向量
            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // 只有距离大于一定值才移动，防止怪物和玩家重合时发生剧烈抖动
            if (dist > 0.1f)
            {
                float speed = stats.MoveSpeed;
                
                // 应用减速组件的影响
                if (enemy.HasComponent<SlowEffectComponent>())
                {
                    speed *= (1f - enemy.GetComponent<SlowEffectComponent>().SlowRatio);
                }

                // 将方向向量归一化后，乘以移动速度
                vel.VX = (dx / dist) * speed;
                vel.VY = (dy / dist) * speed;
            }
        }
    }
}