using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase 
{
    private ContactFilter2D _filter;
    private RaycastHit2D[] _results = new RaycastHit2D[5];

    public BulletCollisionSystem(List<Entity> entities, GridSystem grid) : base(entities) 
    { 
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _filter.useTriggers = true;
    }

    public override void Update(float deltaTime) 
    {
        Physics2D.SyncTransforms();
        // 筛选拥有 轨迹追踪、自动烘焙后的半径组件 和 物理组件 的子弹
        var bullets = GetEntitiesWith<BulletTag, PositionComponent, TraceComponent, CollisionComponent, PhysicsColliderComponent>();

        foreach (var b in bullets) 
        {
            if (!b.IsAlive) continue;

            var pos = b.GetComponent<PositionComponent>();
            var trace = b.GetComponent<TraceComponent>();
            var col = b.GetComponent<CollisionComponent>(); 
            
            Vector2 start = new Vector2(trace.PreviousX, trace.PreviousY);
            Vector2 end = new Vector2(pos.X, pos.Y);
            Vector2 direction = end - start;
            float distance = direction.magnitude;

            int hitCount = 0;
            
            // --- 核心修复：使用烘焙出的半径进行 CircleCast，防止穿透 ---
            if (distance > 0.001f)
            {
                // 检测一个“有厚度”的圆柱弹道
                hitCount = Physics2D.CircleCast(start, col.Radius, direction.normalized, _filter, _results, distance);
            }
            else
            {
                // 初始生成帧执行重叠检测
                var bPhys = b.GetComponent<PhysicsColliderComponent>();
                Collider2D[] overlapResults = new Collider2D[1];
                if (bPhys.Collider.OverlapCollider(_filter, overlapResults) > 0)
                {
                    HandleHit(b, overlapResults[0].gameObject);
                    continue;
                }
            }
            
            if (hitCount > 0)
            {
                HandleHit(b, _results[0].collider.gameObject);
            }
        }
    }

    private void HandleHit(Entity bullet, GameObject hitGo)
    {
        Entity enemy = ECSManager.Instance.GetEntityFromGameObject(hitGo);
        if (enemy != null && enemy.IsAlive)
        {
            bullet.AddComponent(new BulletHitEventComponent(enemy));
        }
    }
}