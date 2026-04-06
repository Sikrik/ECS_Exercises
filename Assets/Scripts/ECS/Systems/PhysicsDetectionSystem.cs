using System.Collections.Generic;
using UnityEngine;

public class PhysicsDetectionSystem : SystemBase
{
    private Collider2D[] _overlapResults = new Collider2D[10];
    private RaycastHit2D[] _castResults = new RaycastHit2D[5];

    public PhysicsDetectionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 强制同步位移后的 Transform
        Physics2D.SyncTransforms();
        
        // 筛选：带物理组件 且 带碰撞过滤器 的实体 (玩家、怪物、子弹现在都带这个)
        var physicsEntities = GetEntitiesWith<PhysicsColliderComponent, PositionComponent, CollisionFilterComponent>();

        foreach (var entity in physicsEntities)
        {
            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            var filter = entity.GetComponent<CollisionFilterComponent>();
            if (pPhys.Collider == null) continue;

            // 构建 Unity 物理过滤器
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(filter.LayerMask);
            contactFilter.useTriggers = true;

            // 1. 高速物体检测 (子弹专用，防止穿墙)
            if (entity.HasComponent<TraceComponent>() && entity.HasComponent<CollisionComponent>())
            {
                var pos = entity.GetComponent<PositionComponent>();
                var trace = entity.GetComponent<TraceComponent>();
                var col = entity.GetComponent<CollisionComponent>();

                Vector2 start = new Vector2(trace.PreviousX, trace.PreviousY);
                Vector2 end = new Vector2(pos.X, pos.Y);
                Vector2 dir = end - start;
                float dist = dir.magnitude;

                if (dist > 0.001f && Physics2D.CircleCast(start, col.Radius, dir.normalized, contactFilter, _castResults, dist) > 0)
                {
                    CreateEvent(entity, _castResults[0].collider.gameObject, _castResults[0].normal);
                }
            }
            // 2. 普通物体检测 (玩家、怪物重叠检测)
            else
            {
                int hitCount = pPhys.Collider.OverlapCollider(contactFilter, _overlapResults);
                for (int i = 0; i < hitCount; i++)
                {
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
            // 发现碰撞，挂载事件组件
            source.AddComponent(new CollisionEventComponent(source, target, normal));
        }
    }
}