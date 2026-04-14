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
                        }
                    }
                }
            }
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
                            Vector2 pushNormal = distInfo.normal;
                            
                            // 【核心修复】：如果两个怪物在同一坐标出生，法线会是 0，导致永远挤不开。
                            // 这里强制给一个随机方向，让它们炸开
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
            var pierce = source.GetComponent<PierceComponent>();
            if (pierce != null)
            {
                if (pierce.HitHistory.Contains(target) || pierce.HitHistory.Count >= pierce.MaxPierces) 
                    return;
                pierce.HitHistory.Add(target);
            }

            Entity eventEntity = ECSManager.Instance.CreateEntity();
            
            // 👇 【修复】：使用泛型对象池获取，并手动赋值
            var colEvt = EventPool<CollisionEventComponent>.Get();
            colEvt.Source = source;
            colEvt.Target = target;
            colEvt.Normal = normal;
            
            eventEntity.AddComponent(colEvt);
            eventEntity.AddComponent(new PendingDestroyComponent()); 
        }
    }
}