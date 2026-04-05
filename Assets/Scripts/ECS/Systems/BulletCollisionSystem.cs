using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase 
{
    private GridSystem _grid;
    public BulletCollisionSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }

    public override void Update(float deltaTime) 
    {
        // 筛选带有标签、位置和半径的子弹
        var bullets = GetEntitiesWith<BulletTag, PositionComponent, CollisionComponent>();

        for (int i = bullets.Count - 1; i >= 0; i--) 
        {
            var b = bullets[i];
            if (!b.IsAlive) continue;

            var bPos = b.GetComponent<PositionComponent>();
            var bCol = b.GetComponent<CollisionComponent>();

            // 空间优化：只检查网格附近的敌人
            var nearbyEnemies = _grid.GetNearbyEnemies(bPos.X, bPos.Y);

            foreach (var enemy in nearbyEnemies) 
            {
                // 确保目标是存活的敌人 (带有 EnemyTag)
                if (!enemy.IsAlive || !enemy.HasComponent<EnemyTag>()) continue;
                
                var ePos = enemy.GetComponent<PositionComponent>();
                var eCol = enemy.GetComponent<CollisionComponent>();

                float r = bCol.Radius + eCol.Radius;
                // 平方距离检测
                float dx = ePos.X - bPos.X;
                float dy = ePos.Y - bPos.Y;
                
                if ((dx * dx + dy * dy) <= (r * r)) 
                {
                    // 命中！挂载命中事件组件，由 BulletEffectSystem 处理具体伤害
                    b.AddComponent(new BulletHitEventComponent(enemy));
                    break; 
                }
            }
        }
    }
}