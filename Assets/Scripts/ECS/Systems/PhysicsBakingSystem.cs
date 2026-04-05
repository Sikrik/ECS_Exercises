using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<ViewComponent, NeedsBakingTag>();

        foreach (var entity in entities)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject == null) continue;

            var col = view.GameObject.GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true; 
                entity.AddComponent(new PhysicsColliderComponent(col));
                
                // --- 核心修复：自动烘焙半径数据 ---
                // 如果预制体上有圆形碰撞体，提取其考虑缩放后的真实半径
                if (col is CircleCollider2D circle)
                {
                    float worldRadius = circle.radius * Mathf.Max(view.GameObject.transform.lossyScale.x, view.GameObject.transform.lossyScale.y);
                    entity.AddComponent(new CollisionComponent(worldRadius));
                }
                // 如果是方块或其他形状，也可以计算一个近似半径，或在此扩展 BoxCast 逻辑
                
                ECSManager.Instance.RegisterEntityView(view.GameObject, entity);
            }

            if (view.Prefab != null && view.Prefab.TryGetComponent<SpriteRenderer>(out var prefabSr))
            {
                if (view.GameObject.TryGetComponent<SpriteRenderer>(out var instanceSr))
                    instanceSr.color = prefabSr.color;
                entity.AddComponent(new BaseColorComponent(prefabSr.color));
            }

            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}