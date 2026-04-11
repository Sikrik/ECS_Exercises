using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 终极位移系统（Motor）
/// 职责：仲裁物理与输入状态，计算最终速度，并执行位移 (高内聚改造版)
/// </summary>
public class MovementSystem : SystemBase
{
    public MovementSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<PositionComponent, VelocityComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            // 1. 记录上一帧坐标（防穿透检测依赖）
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // 2. 速度控制权仲裁
            if (entity.HasComponent<KnockbackComponent>())
            {
                // 最高优先级：被击退中。剥夺输入控制权
            }
            else if (entity.HasComponent<HitRecoveryComponent>())
            {
                // 次高优先级：硬直中。强行刹车，不可移动
                vel.VX = 0;
                vel.VY = 0;
            }
            // 冲刺状态接管速度控制权
            else if (entity.HasComponent<DashStateComponent>())
            {
                var dash = entity.GetComponent<DashStateComponent>();
                var ability = entity.GetComponent<DashAbilityComponent>();
                
                // 覆盖速度：朝向冲刺方向施加固定冲刺速度
                vel.VX = dash.DirX * ability.DashSpeed;
                vel.VY = dash.DirY * ability.DashSpeed;
            }
            else 
            {
                // 普通状态：读取输入意图
                var input = entity.GetComponent<MoveInputComponent>();
                var speed = entity.GetComponent<SpeedComponent>();
                
                if (input != null && speed != null)
                {
                    Vector2 dir = new Vector2(input.X, input.Y);
                    if (dir.sqrMagnitude > 0.001f) dir.Normalize();
                    
                    vel.VX = dir.x * speed.CurrentSpeed;
                    vel.VY = dir.y * speed.CurrentSpeed;
                }
            }

            // 3. 执行最终位移
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 【高内聚改造】：相机的表现层逻辑已彻底抽离，移交至 Presentation 组
        }
        ReturnListToPool(entities);
    }
}