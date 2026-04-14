// 路径: Assets/Scripts/ECS/Systems/Physics/MovementSystem.cs
using System.Collections.Generic;
using UnityEngine;

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
            
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // ==========================================
            // 👇 【高内聚改造】：统一的速度仲裁阶梯
            // ==========================================
            if (entity.HasComponent<KnockbackComponent>())
            {
                // 1. 最高优先级：被击退中。完全剥夺控制权，交由 KnockbackSystem 处理摩擦力衰减
            }
            else if (entity.HasComponent<HitRecoveryComponent>())
            {
                // 2. 次高优先级：受击硬直。强行死锁刹车
                vel.VX = 0;
                vel.VY = 0;
            }
            else if (entity.HasComponent<DashStateComponent>())
            {
                // 3. 冲刺状态：覆盖为固定冲刺速度
                var dash = entity.GetComponent<DashStateComponent>();
                var ability = entity.GetComponent<DashAbilityComponent>();
                vel.VX = dash.DirX * ability.DashSpeed;
                vel.VY = dash.DirY * ability.DashSpeed;
            }
            else if (entity.HasComponent<ShootPrepStateComponent>() || entity.HasComponent<DashPrepStateComponent>())
            {
                // 4. 【新增】：蓄力/前摇状态。无视移动输入，执行平滑刹车衰减（保留少量惯性手感更好）
                float brakeAcceleration = 20f; 
                vel.VX = Mathf.Lerp(vel.VX, 0, brakeAcceleration * deltaTime);
                vel.VY = Mathf.Lerp(vel.VY, 0, brakeAcceleration * deltaTime);
            }
            else 
            {
                // 5. 普通状态：读取输入意图并应用惯性
                var input = entity.GetComponent<MoveInputComponent>();
                var speed = entity.GetComponent<SpeedComponent>();
                
                if (input != null && speed != null)
                {
                    Vector2 dir = new Vector2(input.X, input.Y);
                    if (dir.sqrMagnitude > 0.001f) dir.Normalize();
                    
                    float targetVX = dir.x * speed.CurrentSpeed;
                    float targetVY = dir.y * speed.CurrentSpeed;

                    float acceleration = 25f;
                    var hp = entity.GetComponent<HealthComponent>();
                    
                    if (hp != null)
                    {
                        acceleration = Mathf.Clamp(800f / hp.MaxHealth, 2f, 30f);
                        if (entity.HasComponent<PlayerTag>())
                        {
                            acceleration = Mathf.Clamp(10000f / hp.MaxHealth, 10f, 25f); 
                        }
                    }

                    vel.VX = Mathf.Lerp(vel.VX, targetVX, acceleration * deltaTime);
                    vel.VY = Mathf.Lerp(vel.VY, targetVY, acceleration * deltaTime);
                }
            }

            // 最终位移
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;
        }
    }
}