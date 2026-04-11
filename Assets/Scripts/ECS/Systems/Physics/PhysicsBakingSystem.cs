using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理烘焙系统 (属于 Simulation 逻辑组)
/// 职责：绝对的高内聚，只负责扫描物体的 Collider，生成 ECS 物理组件并注册映射字典。
/// </summary>
public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 改为查询新的物理烘焙标签
        var pending = GetEntitiesWith<NeedsPhysicsBakingTag, ViewComponent>();
        
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            var entity = pending[i];
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject == null) continue;

            // 1. 物理组件烘焙 (支持子物体)
            var col = view.GameObject.GetComponentInChildren<Collider2D>();
            if (col != null)
            {
                entity.AddComponent(new PhysicsColliderComponent(col));
                // 注册发生碰撞的实际物体映射，确保物理检测能找回实体
                ECSManager.Instance.RegisterEntityView(col.gameObject, entity);
                
                float maxScale = Mathf.Max(view.GameObject.transform.lossyScale.x, view.GameObject.transform.lossyScale.y);
                
                if (col is CircleCollider2D circle)
                {
                    float r = circle.radius * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
                else if (col is BoxCollider2D box)
                {
                    float r = Mathf.Max(box.size.x, box.size.y) * 0.5f * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
                else if (col is CapsuleCollider2D capsule)
                {
                    float r = Mathf.Max(capsule.size.x, capsule.size.y) * 0.5f * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
            }

            // 2. 烘焙完成，移除标签
            entity.RemoveComponent<NeedsPhysicsBakingTag>();
        }
    }
}