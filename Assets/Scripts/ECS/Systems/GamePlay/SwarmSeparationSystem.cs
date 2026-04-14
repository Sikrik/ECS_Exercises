// 路径: Assets/Scripts/ECS/Systems/Physics/SwarmSeparationSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class SwarmSeparationSystem : SystemBase
{
    public SwarmSeparationSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 任何有移动意图且需要排斥的实体（不局限于敌人）
        var swarmEntities = GetEntitiesWith<SwarmSeparationComponent, MoveInputComponent, PositionComponent>();

        foreach (var entity in swarmEntities)
        {
            var swarm = entity.GetComponent<SwarmSeparationComponent>();
            var pos = entity.GetComponent<PositionComponent>();
            var input = entity.GetComponent<MoveInputComponent>();

            Vector2 currentPos = new Vector2(pos.X, pos.Y);
            Vector2 desiredDirection = new Vector2(input.X, input.Y);
            Vector2 avoidanceDirection = Vector2.zero;

            // 获取周围的实体
            var nearby = ECSManager.Instance.Grid.GetNearbyEntities(pos.X, pos.Y, 1);
            
            foreach (var other in nearby)
            {
                if (other == entity || !other.HasComponent<EnemyTag>()) continue;

                var otherPos = other.GetComponent<PositionComponent>();
                Vector2 diff = currentPos - new Vector2(otherPos.X, otherPos.Y);
                float sqrDist = diff.sqrMagnitude;

                // 2.25f 是排斥半径的平方
                if (sqrDist < 2.25f && sqrDist > 0.001f)
                {
                    avoidanceDirection += diff.normalized / sqrDist;
                }
            }

            if (avoidanceDirection != Vector2.zero)
            {
                if (desiredDirection == Vector2.zero)
                {
                    desiredDirection = avoidanceDirection.normalized;
                }
                else
                {
                    desiredDirection = (desiredDirection + avoidanceDirection * swarm.SeparationWeight).normalized;
                }

                // 覆写意图，供下一环节的 MovementSystem 消费
                input.X = desiredDirection.x;
                input.Y = desiredDirection.y;
            }
        }
    }
}