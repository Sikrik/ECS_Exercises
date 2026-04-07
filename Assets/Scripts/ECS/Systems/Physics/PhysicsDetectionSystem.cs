using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理碰撞检测系统
/// 【终极优化版】：单次查找 + 对象池 0 GC + 列表回收
/// </summary>
public class PhysicsDetectionSystem : SystemBase
{
    private Collider2D[] _overlapResults = new Collider2D[10];
    private RaycastHit2D[] _castResults = new RaycastHit2D[5];

    public PhysicsDetectionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 强制同步位移后的 Transform
        Physics2D.SyncTransforms();
        
        // 筛选：带物理组件 且 带碰撞过滤器的实体
        var physicsEntities = GetEntitiesWith<PhysicsColliderComponent, PositionComponent, CollisionFilterComponent>();

        // 👇 优化1：推荐使用倒序遍历，养成 ECS 的好习惯
        for (int i = physicsEntities.Count - 1; i >= 0; i--)
        {
            var entity = physicsEntities[i];
            
            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            var filter = entity.GetComponent<CollisionFilterComponent>();
            if (pPhys.Collider == null) continue;

            // 构建 Unity 物理过滤器 (ContactFilter2D 是 struct 结构体，在栈上分配，不会产生 GC)
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(filter.LayerMask);
            contactFilter.useTriggers = true;

            // 👇 优化2：单次查找替代 HasComponent，性能翻倍
            var trace = entity.GetComponent<TraceComponent>();
            var col = entity.GetComponent<CollisionComponent>();

            // 1. 高速物体检测 (子弹专用，防止穿墙)
            if (trace != null && col != null)
            {
                var pos = entity.GetComponent<PositionComponent>();

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
                for (int j = 0; j < hitCount; j++)
                {
                    ColliderDistance2D distInfo = pPhys.Collider.Distance(_overlapResults[j]);
                    if (distInfo.isOverlapped)
                    {
                        CreateEvent(entity, _overlapResults[j].gameObject, distInfo.normal);
                    }
                }
            }
        }
        
        // 👇 优化3：养成好习惯，用完的 List 还给 ECSManager 的对象池
        ReturnListToPool(physicsEntities);
    }

    private void CreateEvent(Entity source, GameObject targetGo, Vector2 normal)
    {
        Entity target = ECSManager.Instance.GetEntityFromGameObject(targetGo);
        if (target != null && target.IsAlive)
        {
            // 👇 终极优化4：抛弃 new，使用对象池借用组件！实现完美的 0 GC！
            source.AddComponent(EventPool.GetCollisionEvent(source, target, normal));
        }
    }
}