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
            
            // 1. 轨迹追踪：更新旧坐标（用于 PhysicsDetectionSystem 的射线检测防穿透）
            if (entity.HasComponent<TraceComponent>())
            {
                var trace = entity.GetComponent<TraceComponent>();
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }
            
            // 2. 逻辑位移计算
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;
        }

        // 处理相机跟随逻辑（仅针对 Player）
        UpdateCameraFollow();

        ReturnListToPool(entities);
    }

    private void UpdateCameraFollow()
    {
        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count > 0)
        {
            var pPos = players[0].GetComponent<PositionComponent>();
            Camera camera = Camera.main;
            if (camera != null)
            {
                Vector3 target = new Vector3(pPos.X, pPos.Y, camera.transform.position.z);
                camera.transform.position = Vector3.Lerp(camera.transform.position, target, 0.1f);
            }
        }
        ReturnListToPool(players);
    }
}