using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase
{
    public BulletCollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var bullets = GetEntitiesWith<BulletComponent, PositionComponent, CollisionComponent>();
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent>();

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            if (!bullet.IsAlive) continue;

            var bPos = bullet.GetComponent<PositionComponent>();
            var bCol = bullet.GetComponent<CollisionComponent>();

            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                var enemy = enemies[j];
                if (!enemy.IsAlive) continue;

                var ePos = enemy.GetComponent<PositionComponent>();
                var eCol = enemy.GetComponent<CollisionComponent>();

                float r = bCol.Radius + eCol.Radius;
                if (CheckSegmentCollision(bPos.PreviousX, bPos.PreviousY, bPos.X, bPos.Y, ePos.X, ePos.Y, r))
                {
                    bullet.AddComponent(new BulletHitEventComponent(enemy));
                    break; 
                }
            }
        }
    }

    private bool CheckSegmentCollision(float p1x, float p1y, float p2x, float p2y, float cx, float cy, float r)
    {
        float dx = p2x - p1x; float dy = p2y - p1y;
        float d2 = dx * dx + dy * dy;
        float t = d2 < 0.0001f ? 0 : Mathf.Clamp01(((cx - p1x) * dx + (cy - p1y) * dy) / d2);
        float closeX = p1x + t * dx; float closeY = p1y + t * dy;
        return (cx - closeX) * (cx - closeX) + (cy - closeY) * (cy - closeY) <= r * r;
    }
}