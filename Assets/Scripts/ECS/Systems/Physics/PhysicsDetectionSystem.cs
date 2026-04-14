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
            // 1. 连续碰撞检测 (主要用于高速子弹)
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
                            CreateEvent(entity, _castResults[j].collider.gameObject, dir.normalized);
                        }
                    }
                }
            }
            // ==========================================
            // 2. 离散碰撞检测 (主要用于肉体碰撞与穿模修复)
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
                            // Unity法线从目标指向源，取反得到推开目标的方向
                            Vector2 pushNormal = -distInfo.normal; 
                            
                            if (pushNormal == Vector2.zero)
                            {
                                pushNormal = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                            }

                            // 👇 【核心修复：防止穿模排斥】
                            // 计算重叠深度 (-distance)
                            float penetrationDepth = -distInfo.distance;
                            
                            // 确定权重：如果撞到的是墙（没有Velocity），自己退100%；如果是单位对撞，各退50%
                            float moveRatio = 1.0f;
                            Entity targetEnt = ECSManager.Instance.GetEntityFromGameObject(_overlapResults[j].gameObject);
                            if (targetEnt != null && targetEnt.HasComponent<VelocityComponent>())
                            {
                                moveRatio = 0.5f;
                            }

                            // 立即修正坐标，解决“钻到底下”和“重叠”问题
                            var pos = entity.GetComponent<PositionComponent>();
                            if (pos != null)
                            {
                                pos.X += pushNormal.x * penetrationDepth * moveRatio;
                                pos.Y += pushNormal.y * penetrationDepth * moveRatio;
                            }

                            // 只有在对方没有无敌帧时，才生成伤害事件 (防止无敌期间无限产生排斥力)
                            if (targetEnt == null || !targetEnt.HasComponent<InvincibleComponent>())
                            {
                                CreateEvent(entity, _overlapResults[j].gameObject, pushNormal);
                            }
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
            // 防止敌人把子弹当成受害者
            if (source.HasComponent<EnemyTag>() && target.HasComponent<BulletTag>()) return;

            // 同阵营免疫
            var sFac = source.GetComponent<FactionComponent>();
            var tFac = target.GetComponent<FactionComponent>();
            if (sFac != null && tFac != null && sFac.Value == tFac.Value) return;

            // 穿透逻辑处理
            var pierce = source.GetComponent<PierceComponent>();
            if (pierce != null)
            {
                if (pierce.HitHistory.Contains(target) || pierce.HitHistory.Count >= pierce.MaxPierces) 
                    return;
                pierce.HitHistory.Add(target);
            }

            // 生成碰撞事件实体
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