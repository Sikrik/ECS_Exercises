using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动执行系统：负责最终物理坐标的累加和相机跟随
/// 注：旧版的击退摩擦力逻辑已移除，交由 KnockbackSystem 的 Lerp 平滑接管
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
            
            // 1. 记录上一帧的坐标（非常重要：用于高速子弹的 TraceComponent 防穿透射线检测）
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // 2. 将本帧所有系统（输入、AI、击退等）计算出的最终速度，应用到空间坐标上
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 3. 玩家的专属逻辑：让相机平滑地跟上玩家的最新坐标
            if (entity.HasComponent<PlayerTag>())
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    Vector3 target = new Vector3(pos.X, pos.Y, mainCam.transform.position.z);
                    // 0.1f 的 Lerp 插值能让相机跟随带有微微的阻尼感，不至于太死板
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, target, 0.1f);
                }
            }
        }
        
        // 保持 0 GC 设计：归还临时查询列表
        ReturnListToPool(entities);
    }
}