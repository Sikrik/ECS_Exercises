using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase {
    private GridSystem _grid;
    public BulletCollisionSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }

    public override void Update(float deltaTime) {
        var bullets = GetEntitiesWith<BulletComponent, PositionComponent, CollisionComponent>();

        for (int i = bullets.Count - 1; i >= 0; i--) {
            var b = bullets[i];
            if (!b.IsAlive) continue;

            var bPos = b.GetComponent<PositionComponent>();
            var bCol = b.GetComponent<CollisionComponent>();

            // 关键优化：只从网格中获取附近的敌人
            var nearbyEnemies = _grid.GetNearbyEnemies(bPos.X, bPos.Y);

            foreach (var enemy in nearbyEnemies) {
                if (!enemy.IsAlive) continue;
                var ePos = enemy.GetComponent<PositionComponent>();
                var eCol = enemy.GetComponent<CollisionComponent>();

                float r = bCol.Radius + eCol.Radius;
                // 使用平方距离比较，避免开根号
                if (CheckCollision(bPos, ePos, r)) {
                    b.AddComponent(new BulletHitEventComponent(enemy));
                    break; 
                }
            }
        }
    }

    private bool CheckCollision(PositionComponent b, PositionComponent e, float r) {
        float dx = e.X - b.X; float dy = e.Y - b.Y;
        return (dx * dx + dy * dy) <= (r * r);
    }
}