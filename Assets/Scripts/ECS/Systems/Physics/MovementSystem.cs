using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 终极位移系统（Motor）
/// 职责：仲裁物理与输入状态，计算最终速度，并执行平滑位移（惯性系统）
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
                // 普通状态：读取输入意图并应用惯性
                var input = entity.GetComponent<MoveInputComponent>();
                var speed = entity.GetComponent<SpeedComponent>();
                
                if (input != null && speed != null)
                {
                    Vector2 dir = new Vector2(input.X, input.Y);
                    if (dir.sqrMagnitude > 0.001f) dir.Normalize();
                    
                    // 计算当前的“期望目标速度”
                    float targetVX = dir.x * speed.CurrentSpeed;
                    float targetVY = dir.y * speed.CurrentSpeed;

                    // 惯性系统：基于最大血量计算加速度
                    float acceleration = 25f; // 默认极快响应（无惯性）
                    var hp = entity.GetComponent<HealthComponent>();
                    
                    if (hp != null)
                    {
                        // 核心机制：最大血量越大，加速度越小，惯性越强！
                        // 限制在 [2f, 30f] 区间，防止极端数值导致无法控制
                        acceleration = Mathf.Clamp(800f / hp.MaxHealth, 2f, 30f);
                        
                        // 玩家专属手感特调，保留丝滑感且不易失控
                        if (entity.HasComponent<PlayerTag>())
                        {
                            acceleration = Mathf.Clamp(10000f / hp.MaxHealth, 10f, 25f); 
                        }
                    }

                    // 执行最终速度的平滑过渡
                    vel.VX = Mathf.Lerp(vel.VX, targetVX, acceleration * deltaTime);
                    vel.VY = Mathf.Lerp(vel.VY, targetVY, acceleration * deltaTime);
                }
            }

            // 3. 执行最终位移
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;
        }
        ReturnListToPool(entities);
    }
}