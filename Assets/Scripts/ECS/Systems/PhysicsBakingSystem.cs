using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var pending = GetEntitiesWith<NeedsBakingTag, ViewComponent>();

        foreach (var entity in pending)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                var col = view.GameObject.GetComponentInChildren<Collider2D>();
                if (col != null)
                {
                    entity.AddComponent(new PhysicsColliderComponent(col));
                    // --- 核心修复 3：注册碰撞体所在的物体，确保能反向查找到实体 ---
                    ECSManager.Instance.RegisterEntityView(col.gameObject, entity);
                
                    if (entity.HasComponent<BulletTag>())
                    {
                        float r = 0.2f;
                        if (col is CircleCollider2D circle) r = circle.radius * view.GameObject.transform.lossyScale.x;
                        entity.AddComponent(new CollisionComponent(r));
                    }
                }
            }
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}