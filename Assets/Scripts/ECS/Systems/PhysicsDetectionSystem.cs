using System.Collections.Generic;
using UnityEngine;

public class PhysicsDetectionSystem : SystemBase
{
    private Collider2D[] _overlapResults = new Collider2D[10];
    private RaycastHit2D[] _castResults = new RaycastHit2D[5];
    private ContactFilter2D _enemyFilter;

    public PhysicsDetectionSystem(List<Entity> entities) : base(entities)
    {
        _enemyFilter = new ContactFilter2D();
        _enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy")); // 后续可改为从组件读取 Mask
        _enemyFilter.useTriggers = true;
    }

    public override void Update(float deltaTime)
    {
        Physics2D.SyncTransforms();
        
        // 筛选所有带物理组件的实体（包括玩家和子弹）
        var physicsEntities = GetEntitiesWith<PhysicsColliderComponent, PositionComponent>();

        foreach (var entity in physicsEntities)
        {
            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            if (pPhys.Collider == null) continue;

            // 1. 高速物体检测 (如子弹，使用射线/圆柱投射防止穿透)
            if (entity.HasComponent<TraceComponent>() && entity.HasComponent<CollisionComponent>())
            {
                var pos = entity.GetComponent<PositionComponent>();
                var trace = entity.GetComponent<TraceComponent>();
                var col = entity.GetComponent<CollisionComponent>();

                Vector2 start = new Vector2(trace.PreviousX, trace.PreviousY);
                Vector2 end = new Vector2(pos.X, pos.Y);
                Vector2 dir = end - start;
                float dist = dir.magnitude;

                if (dist > 0.001f && Physics2D.CircleCast(start, col.Radius, dir.normalized, _enemyFilter, _castResults, dist) > 0)
                {
                    CreateEvent(entity, _castResults[0].collider.gameObject, _castResults[0].normal);
                }
            }
            // 2. 普通物体检测 (如玩家，使用重叠检测)
            else
            {
                int hitCount = pPhys.Collider.OverlapCollider(_enemyFilter, _overlapResults);
                for (int i = 0; i < hitCount; i++)
                {
                    // 计算法线以便后续反弹使用
                    ColliderDistance2D distInfo = pPhys.Collider.Distance(_overlapResults[i]);
                    if (distInfo.isOverlapped)
                    {
                        CreateEvent(entity, _overlapResults[i].gameObject, distInfo.normal);
                    }
                }
            }
        }
    }

    private void CreateEvent(Entity source, GameObject targetGo, Vector2 normal)
    {
        Entity target = ECSManager.Instance.GetEntityFromGameObject(targetGo);
        if (target != null && target.IsAlive)
        {
            // 发现碰撞，只记录事件，不处理逻辑
            source.AddComponent(new CollisionEventComponent(source, target, normal));
        }
    }
}