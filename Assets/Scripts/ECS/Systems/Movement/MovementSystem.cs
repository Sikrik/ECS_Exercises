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
            
            // 1. 更新防穿透检测的上一帧位置
            // 必须在坐标改变前更新，确保 PhysicsDetectionSystem 里的射线段长度正确
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // 2. 【手感优化】：模拟滑动摩擦力
            // 只要实体处于击退状态（无论是玩家还是怪物），都会产生阻力
            if (entity.HasComponent<KnockbackComponent>())
            {
                // 系数 0.85 会产生更明显的“肉感”阻力，比 0.92 更干脆
                vel.VX *= 0.85f; 
                vel.VY *= 0.85f;
            }
            
            // 3. 执行位移计算
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 4. 相机跟随逻辑
            // 维持原有逻辑，将主相机平滑移动到玩家坐标
            if (entity.HasComponent<PlayerTag>())
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 target = new Vector3(pos.X, pos.Y, mainCam.transform.position.z);
                    // 0.1f 的插值让相机跟随带有一定的滞后感，更舒适
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, target, 0.1f);
                }
            }
        }
        // 归还列表到池，维持战斗 0 GC
        ReturnListToPool(entities);
    }
}