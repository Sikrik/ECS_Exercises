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
            if (view.GameObject == null) continue;

            // 核心修复：使用 GetComponentInChildren 兼容子物体碰撞体
            var col = view.GameObject.GetComponentInChildren<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true; 
                entity.AddComponent(new PhysicsColliderComponent(col));
            
                // 核心修复：注册【碰撞体所在的那个物体】，确保物理检测能找回实体
                ECSManager.Instance.RegisterEntityView(col.gameObject, entity);
            
                // 计算逻辑半径 (如果是圆形)
                if (col is CircleCollider2D circle)
                {
                    float worldRadius = circle.radius * Mathf.Max(view.GameObject.transform.lossyScale.x, view.GameObject.transform.lossyScale.y);
                    entity.AddComponent(new CollisionComponent(worldRadius));
                }
            }
        
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}