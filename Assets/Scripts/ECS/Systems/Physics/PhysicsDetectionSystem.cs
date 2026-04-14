// 路径: Assets/Scripts/ECS/Systems/Physics/PhysicsDetectionSystem.cs
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
            if (!entity.IsAlive || entity.HasComponent<PendingDestroyComponent>()) continue;

            var pPhys = entity.GetComponent<PhysicsColliderComponent>();
            var filter = entity.GetComponent<CollisionFilterComponent>();
            if (pPhys.Collider == null) continue;

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(filter.LayerMask);
            contactFilter.useTriggers = true;

            var trace = entity.GetComponent<TraceComponent>();
            var col = entity.GetComponent<CollisionComponent>();

            // ==========================================
            // 1. 连续碰撞检测 (主要用于子弹)
            // ==========================================
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
                            // 【修复1】：子弹的击退方向应该完全遵循子弹的飞行方向，而不是敌人的表面法线
                            CreateEvent(entity, _castResults[j].collider.gameObject, dir.normalized);
                        }
                    }
                }
            }
            // ==========================================
            // 2. 离散碰撞检测 (主要用于肉体冲撞)
            // ==========================================
            else 
            {
                int hitCount = pPhys.Collider.OverlapCollider(contactFilter, _overlapResults);
                for (int j = 0; j < hitCount; j++)
                {
                    if (_overlapResults[j] != pPhys.Collider)
                    {
                        ColliderDistance2D distInfo = pPhys.Collider.Distance(_overlapResults[j]);
                        if (distInfo.isOverlapped)
                        {
                            // 【修复2】：Unity的法线是从 Target 指向 Source。为了把 Target 推开，必须取反！
                            Vector2 pushNormal = -distInfo.normal; 
                            
                            if (pushNormal == Vector2.zero)
                            {
                                pushNormal = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                            }
                            CreateEvent(entity, _overlapResults[j].gameObject, pushNormal);
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
            // 【修复3】：防止敌人主动把“子弹”当成受害者，反向给子弹施加硬直和击退！
            if (source.HasComponent<EnemyTag>() && target.HasComponent<BulletTag>()) return;

            var pierce = source.GetComponent<PierceComponent>();
            if (pierce != null)
            {
                if (pierce.HitHistory.Contains(target) || pierce.HitHistory.Count >= pierce.MaxPierces) 
                    return;
                pierce.HitHistory.Add(target);
            }

            Entity eventEntity = ECSManager.Instance.CreateEntity();
            var colEvt = EventPool<CollisionEventComponent>.Get();
            colEvt.Source = source;
            colEvt.Target = target;
            colEvt.Normal = normal;
            
            eventEntity.AddComponent(colEvt);
            eventEntity.AddComponent(new PendingDestroyComponent()); 
        }
    }
}