using System.Collections.Generic;
using UnityEngine;
// MovementSystem.cs 完整代码（仅保留相机跟随，删除玩家边界限制）
public class MovementSystem : SystemBase
{
    public MovementSystem(List<Entity> entities) : base(entities) { }
    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<PositionComponent, VelocityComponent>();
        // 获取主相机，用于后续跟随逻辑
        var camera = Camera.main;
        
        // 初始化玩家位置（用于相机跟随）
        Vector3 playerPosition = Vector3.zero;
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            // 保存上一帧的位置，用于高速物体的碰撞检测，解决穿透问题
            pos.PreviousX = pos.X;
            pos.PreviousY = pos.Y;
            
            // 原有移动逻辑（保留，玩家可自由移动，无边界限制）
            pos.X += vel.SpeedX * deltaTime;
            pos.Y += vel.SpeedY * deltaTime;
            
            // 仅记录玩家当前位置，用于相机跟随（删除边界限制代码）
            if (entity.HasComponent<PlayerComponent>())
            {
                playerPosition = new Vector3(pos.X, pos.Y, camera.transform.position.z);
            }
        }
        
        // 相机跟随逻辑：平滑跟随玩家，保持相机Z轴不变（避免画面拉伸）
        if (camera != null)
        {
            // 平滑插值，参数0.1f为跟随灵敏度，值越小跟随越平缓（可在配置中添加，方便调整）
            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                playerPosition,
                0.1f
            );
        }
    }
}