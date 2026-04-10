using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动执行系统：负责最终物理坐标的累加和滑动摩擦力模拟
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
            
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // 【手感优化】：阻力回调到 0.92f，保证物理弹飞有足够的滑动距离和表现时间
            if (entity.HasComponent<KnockbackComponent>())
            {
                vel.VX *= 0.92f; 
                vel.VY *= 0.92f;
            }
            
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

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
    }
}