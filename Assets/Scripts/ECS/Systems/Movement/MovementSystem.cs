using System.Collections.Generic;
using UnityEngine;

public class MovementSystem : SystemBase
{
    private Camera _mainCamera; // 缓存相机引用

    public MovementSystem(List<Entity> entities) : base(entities) 
    {
        _mainCamera = Camera.main; // 初始化时仅寻找一次
    }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<PositionComponent, VelocityComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            if (entity.HasComponent<TraceComponent>())
            {
                var trace = entity.GetComponent<TraceComponent>();
                trace.PreviousX = pos.X; trace.PreviousY = pos.Y;
            }
            
            pos.X += vel.VX * deltaTime;
            pos.Y += vel.VY * deltaTime;

            // 相机跟随逻辑使用缓存的 _mainCamera
            if (entity.HasComponent<PlayerTag>() && _mainCamera != null)
            {
                Vector3 target = new Vector3(pos.X, pos.Y, _mainCamera.transform.position.z);
                _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, target, 0.1f);
            }
        }
    }
}