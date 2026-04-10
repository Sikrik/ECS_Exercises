using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 终极位移系统（Motor）
/// 职责：仲裁物理与输入状态，计算最终速度，并执行位移
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

            // 2. 【核心重构：速度控制权仲裁】
            if (entity.HasComponent<KnockbackComponent>())
            {
                // 最高优先级：被击退中。剥夺输入控制权，接管物理摩擦力
                vel.VX = Mathf.Lerp(vel.VX, 0, deltaTime * 15f);
                vel.VY = Mathf.Lerp(vel.VY, 0, deltaTime * 15f);
            }
            else if (entity.HasComponent<HitRecoveryComponent>())
            {
                // 次高优先级：硬直中。强行刹车，不可移动
                vel.VX = 0;
                vel.VY = 0;
            }
            else 
            {
                // 普通状态：读取输入意图（可能是玩家键盘，也可能是怪物AI）
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

            // 4. 相机跟随
            if (entity.HasComponent<PlayerTag>())
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 target = new Vector3(pos.X, pos.Y, mainCam.transform.position.z);
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, target, 0.1f);
                }
            }
        }
        ReturnListToPool(entities);
    }
}