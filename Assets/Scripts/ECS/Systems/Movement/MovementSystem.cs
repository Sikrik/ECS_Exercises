using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : SystemBase
{
    public MovementSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有能动的物体
        var entities = GetEntitiesWith<PositionComponent, VelocityComponent>();
        var camera = Camera.main;
        Vector3 playerPosition = Vector3.zero;
        bool foundPlayer = false;
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            // 1. 轨迹追踪优化：仅针对有 TraceComponent 的物体更新旧位置（防穿透）
            if (entity.HasComponent<TraceComponent>())
            {
                var trace = entity.GetComponent<TraceComponent>();
                trace.PreviousX = pos.X;
                trace.PreviousY = pos.Y;
            }
            
            // 2. 基础位移逻辑
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;
            
            // 3. 相机跟随：使用 PlayerTag 识别玩家
            if (!foundPlayer && entity.HasComponent<PlayerTag>())
            {
                playerPosition = new Vector3(pos.X, pos.Y, camera.transform.position.z);
                foundPlayer = true;
            }
        }
        
        // 平滑跟随逻辑保持不变
        if (foundPlayer && camera != null)
        {
            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                playerPosition,
                0.1f
            );
        }
        ReturnListToPool(entities);
    }
}