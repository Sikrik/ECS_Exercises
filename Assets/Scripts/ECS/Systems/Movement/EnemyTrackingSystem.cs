using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：基于 StatusSummaryComponent 进行寻路决策。
/// 优化版：简化了拦截逻辑，完全信任状态汇总管线 (StatusGatherSystem) 的结果。
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 获取玩家实体，如果玩家不存在或已死亡则不进行追踪
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        
        // 2. 筛选出所有需要寻路的敌人及其必要组件
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent, StatusSummaryComponent>();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            var summary = enemy.GetComponent<StatusSummaryComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();

            // ==========================================
            // 3. 状态拦截优化：职责分离
            // ==========================================
            // 只要 StatusGatherSystem 判定不能移动（硬直/击退/冰冻），这里就停止寻路计算
            if (!summary.CanMove)
            {
                // 只有在没有“击退”等外部物理速度干扰时，才手动归零寻路速度
                // 这样可以确保 KnockbackSystem 施加的推力不会被本系统覆盖
                if (!enemy.HasComponent<KnockbackComponent>()) 
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                continue; 
            }

            // ==========================================
            // 4. 正常寻路计算
            // ==========================================
            var ePos = enemy.GetComponent<PositionComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                // 结合基础速度与由 StatusGatherSystem 计算出的减速倍率
                float finalSpeed = stats.Config.Speed * summary.SpeedMultiplier;
                vel.VX = (dx / dist) * finalSpeed;
                vel.VY = (dy / dist) * finalSpeed;
            }
            else
            {
                vel.VX = 0;
                vel.VY = 0;
            }
        }
        
        // 归还从池中借用的列表，维持 0 GC
        ReturnListToPool(enemies);
    }
}