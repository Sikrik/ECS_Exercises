// Assets/Scripts/ECS/Systems/PlayerControlSystem.cs

using System.Collections.Generic;
using UnityEngine;

public class PlayerControlSystem : SystemBase
{
    public PlayerControlSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        // 筛选：玩家标记 + 移动意图 + 速度组件
        var entities = GetEntitiesWith<PlayerTag, MoveInputComponent, VelocityComponent>();

        foreach (var entity in entities)
        {
            var input = entity.GetComponent<MoveInputComponent>();
            var vel = entity.GetComponent<VelocityComponent>();
            
            Vector2 dir = new Vector2(input.X, input.Y).normalized;
            vel.VX = dir.x * config.PlayerMoveSpeed;
            vel.VY = dir.y * config.PlayerMoveSpeed;
        }
    }
}