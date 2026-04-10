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
            
            // 【核心修改】：模拟滑动摩擦力
            // 只有处于击退滑动状态的非玩家物体会减速
            if (entity.HasComponent<KnockbackComponent>() && !entity.HasComponent<PlayerTag>())
            {
                vel.VX *= 0.92f; // 衰减系数，控制滑动的阻力感
                vel.VY *= 0.92f;
            }
            
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 相机跟随逻辑保持不变
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