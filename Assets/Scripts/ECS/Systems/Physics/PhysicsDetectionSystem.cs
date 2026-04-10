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
            if (entity.HasComponent<PendingDestroyComponent>()) continue;

            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            var filter = entity.GetComponent<CollisionFilterComponent>();
            if (pPhys.Collider == null) continue;

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(filter.LayerMask);
            contactFilter.useTriggers = true;

            var trace = entity.GetComponent<TraceComponent>();
            var col = entity.GetComponent<CollisionComponent>();

            // 1. 高速物体（子弹）使用射线检测防穿透
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
                        if (_castResults[j].collider != pPhys.Collider)
                        {
                            CreateEvent(entity, _castResults[j].collider.gameObject, _castResults[j].normal);
                            break; 
                        }
                    }
                }
            }
            // 2. 普通物体（玩家/敌人）使用重叠检测
            else 
            {
                int hitCount = pPhys.Collider.OverlapCollider(contactFilter, _overlapResults);
                for (int j = 0; j < hitCount; j++)
                {
                    if (_overlapResults[j] != pPhys.Collider)
                    {
                        // 获取两个碰撞体之间的最短距离信息
                        ColliderDistance2D distInfo = pPhys.Collider.Distance(_overlapResults[j]);
                        if (distInfo.isOverlapped)
                        {
                            // 传入正常的碰撞法线（从 B 指向 A）
                            CreateEvent(entity, _overlapResults[j].gameObject, distInfo.normal);
                        }
                    }
                }
            }
        }
    }

    private void CreateEvent(Entity source, GameObject targetGo, Vector2 normal)
    {
        if (source.HasComponent<PendingDestroyComponent>()) return;

        Entity target = ECSManager.Instance.GetEntityFromGameObject(targetGo);
        if (target != null && target.IsAlive && !target.HasComponent<PendingDestroyComponent>())
        {
            Entity eventEntity = ECSManager.Instance.CreateEntity();
            // 使用对象池获取事件组件
            eventEntity.AddComponent(EventPool.GetCollisionEvent(source, target, normal));
            eventEntity.AddComponent(new PendingDestroyComponent()); 
        }
    }
}