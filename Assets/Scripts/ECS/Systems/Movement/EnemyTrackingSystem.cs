using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物追踪系统：负责计算敌人的寻路速度。
/// 优化版：完全解耦了状态逻辑，仅根据 SpeedComponent.CurrentSpeed 执行位移决策。
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
        
        // 2. 筛选所有需要寻路的敌人。
        // 注意：这里不再需要查询 StatusSummaryComponent。
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, SpeedComponent>();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            var vel = enemy.GetComponent<VelocityComponent>();
            var speed = enemy.GetComponent<SpeedComponent>();

            // ==========================================
            // 3. 速度拦截逻辑
            // ==========================================
            // 只要 StatusGatherSystem 判定当前速度为 0（如处于硬直或击退中），此处直接跳过计算。
            if (speed.CurrentSpeed <= 0)
            {
                // 只有在没有“击退”等外部物理速度干扰时，才手动归零寻路产生的速度。
                if (!enemy.HasComponent<KnockbackComponent>()) 
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }
                continue; 
            }

            // ==========================================
            // 4. 寻路向量计算
            // ==========================================
            var ePos = enemy.GetComponent<PositionComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            // 防止抖动：距离目标过近时停止
            if (dist > 0.1f)
            {
                // 直接使用由状态系统计算好的最终实时速度
                vel.VX = (dx / dist) * speed.CurrentSpeed;
                vel.VY = (dy / dist) * speed.CurrentSpeed;
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