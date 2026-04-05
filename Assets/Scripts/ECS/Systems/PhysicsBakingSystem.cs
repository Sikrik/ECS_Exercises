using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 扫描所有带 View 但还没烘焙物理信息的实体
        var entities = GetEntitiesWith<ViewComponent, NeedsBakingTag>();

        foreach (var entity in entities)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject == null) continue;

            // 自动抓取碰撞体（不管你在预制体上挂的是 Box、Circle 还是 Polygon）
            var col = view.GameObject.GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true; // 强制设为 Trigger
                entity.AddComponent(new PhysicsColliderComponent(col));
                
                // 注册到 ECSManager 映射表
                ECSManager.Instance.RegisterEntityView(view.GameObject, entity);
            }

            // 顺便做个“视觉重置”，确保新生的怪颜色是对的
            if (view.Prefab != null && view.Prefab.TryGetComponent<SpriteRenderer>(out var prefabSr))
            {
                if (view.GameObject.TryGetComponent<SpriteRenderer>(out var instanceSr))
                    instanceSr.color = prefabSr.color;
                entity.AddComponent(new BaseColorComponent(prefabSr.color));
            }

            // 烘焙完成，移除标记
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}