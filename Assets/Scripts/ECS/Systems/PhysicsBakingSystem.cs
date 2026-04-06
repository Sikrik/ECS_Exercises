using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有刚生成、等待烘焙物理组件的实体
        var pending = GetEntitiesWith<NeedsBakingTag, ViewComponent>();

        foreach (var entity in pending)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                // --- BUG 修复：支持子物体碰撞体 ---
                var col = view.GameObject.GetComponentInChildren<Collider2D>();
                if (col != null)
                {
                    // 挂载物理组件供 PhysicsDetectionSystem 使用
                    entity.AddComponent(new PhysicsColliderComponent(col));
                    
                    // --- BUG 修复：建立双向映射 ---
                    // 必须注册，物理系统才能在发生碰撞时找回实体
                    ECSManager.Instance.RegisterEntityView(view.GameObject, entity);
                    
                    // 针对子弹等高速物体，自动计算逻辑半径
                    if (entity.HasComponent<BulletTag>())
                    {
                        float radius = 0.2f; // 默认值
                        if (col is CircleCollider2D circle) radius = circle.radius * view.GameObject.transform.localScale.x;
                        entity.AddComponent(new CollisionComponent(radius));
                    }
                }
            }

            // 移除标记，表示烘焙完成
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}