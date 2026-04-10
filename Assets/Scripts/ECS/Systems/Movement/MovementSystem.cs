using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : SystemBase
{
    public MovementSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有位置和速度组件的实体
        var entities = GetEntitiesWith<PositionComponent, VelocityComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            // 【核心修复】：更新防穿透检测的上一帧位置
            // 如果不更新 PreviousX/Y，射线起点将永远停留在开火处，导致子弹路径变成一条无限增长的直线
            var trace = entity.GetComponent<TraceComponent>();
            if (trace != null)
            {
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }

            // 模拟滑动摩擦力：只有处于击退状态且非玩家的物体会减速
            if (entity.HasComponent<KnockbackComponent>() && !entity.HasComponent<PlayerTag>())
            {
                vel.VX *= 0.92f; // 衰减系数，控制滑动的阻力感
                vel.VY *= 0.92f;
            }
            
            // 执行位移计算
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 相机跟随逻辑：仅针对玩家实体
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