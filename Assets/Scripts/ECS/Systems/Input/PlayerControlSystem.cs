using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制系统：负责将输入意图转化为速度
/// 优化：增加了状态锁，确保物理反馈（如击退）不会被键盘输入瞬间覆盖
/// </summary>
public class PlayerControlSystem : SystemBase
{
    public PlayerControlSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选：玩家标记 + 移动意图 + 速度/速率组件
        var entities = GetEntitiesWith<PlayerTag, MoveInputComponent, VelocityComponent, SpeedComponent>();

        foreach (var entity in entities)
        {
            // 【关键手感优化】：状态锁
            // 如果玩家正在被击退滑动，或者处于受击硬直中，暂时剥夺玩家对速度的控制权
            // 这能让玩家感受到被怪物撞飞的“顿挫感”
            if (entity.HasComponent<KnockbackComponent>() || entity.HasComponent<HitRecoveryComponent>())
            {
                continue; 
            }

            var input = entity.GetComponent<MoveInputComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            var speed = entity.GetComponent<SpeedComponent>();
            
            // 计算移动向量并归一化（防止斜向移动变快）
            Vector2 dir = new Vector2(input.X, input.Y);
            if (dir.sqrMagnitude > 0.001f)
            {
                dir.Normalize();
            }

            // 应用当前速度（CurrentSpeed 已经由 StatusGatherSystem 计算了减速效果）
            vel.VX = dir.x * speed.CurrentSpeed;
            vel.VY = dir.y * speed.CurrentSpeed;
        }

        // 归还查询列表
        ReturnListToPool(entities);
    }
}