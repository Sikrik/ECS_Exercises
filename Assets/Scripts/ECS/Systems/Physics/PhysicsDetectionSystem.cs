using System.Collections.Generic;
using UnityEngine;

public class PhysicsDetectionSystem : SystemBase
{
    private Collider2D[] _overlapResults = new Collider2D[10];
    private RaycastHit2D[] _castResults = new RaycastHit2D[5];

    public PhysicsDetectionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        Physics2D.SyncTransforms();
        var physicsEntities = GetEntitiesWith<PhysicsColliderComponent, PositionComponent, CollisionFilterComponent>();

        for (int i = physicsEntities.Count - 1; i >= 0; i--)
        {
            var entity = physicsEntities[i];
            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            var filter = entity.GetComponent<CollisionFilterComponent>();
            if (pPhys.Collider == null) continue;

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(filter.LayerMask);
            contactFilter.useTriggers = true;

            var trace = entity.GetComponent<TraceComponent>();
            var col = entity.GetComponent<CollisionComponent>();

            // 1. 高速物体检测 (子弹专用)
            if (trace != null && col != null)
            {
                var pos = entity.GetComponent<PositionComponent>();
                Vector2 start = new Vector2(trace.PreviousX, trace.PreviousY);
                Vector2 end = new Vector2(pos.X, pos.Y);
                Vector2 dir = end - start;
                float dist = dir.magnitude;

                if (dist > 0.001f)
                {
                    int hitCount = Physics2D.CircleCast(start, col.Radius, dir.normalized, contactFilter, _castResults, dist);
                    for (int j = 0; j < hitCount; j++)
                    {
                        // 🚨 核心修复：绝对不能打中自己的碰撞体！
                        if (_castResults[j].collider != pPhys.Collider)
                        {
                            CreateEvent(entity, _castResults[j].collider.gameObject, _castResults[j].normal);
                            break; 
                        }
                    }
                }
            }
            // 2. 普通物体检测 (玩家、怪物)
            else
            {
                int hitCount = pPhys.Collider.OverlapCollider(contactFilter, _overlapResults);
                for (int j = 0; j < hitCount; j++)
                {
                    // 🚨 防御性编程：排除自己
                    if (_overlapResults[j] != pPhys.Collider)
                    {
                        ColliderDistance2D distInfo = pPhys.Collider.Distance(_overlapResults[j]);
                        if (distInfo.isOverlapped)
                        {
                            CreateEvent(entity, _overlapResults[j].gameObject, distInfo.normal);
                        }
                    }
                }
            }
        }
        
        ReturnListToPool(physicsEntities);
    }

    private void CreateEvent(Entity source, GameObject targetGo, Vector2 normal)
    {
        Entity target = ECSManager.Instance.GetEntityFromGameObject(targetGo);
        if (target != null && target.IsAlive)
        {
            source.AddComponent(EventPool.GetCollisionEvent(source, target, normal));
        }
    }
}