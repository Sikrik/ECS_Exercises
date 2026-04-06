using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出需要烘焙的实体
        var pending = GetEntitiesWith<NeedsBakingTag, ViewComponent>();

        foreach (var entity in pending)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject == null) continue;

            // 1. 物理组件烘焙 (支持子物体)
            var col = view.GameObject.GetComponentInChildren<Collider2D>();
            if (col != null)
            {
                entity.AddComponent(new PhysicsColliderComponent(col));
                // 注册发生碰撞的实际物体映射，确保物理检测能找回实体
                ECSManager.Instance.RegisterEntityView(col.gameObject, entity);
                
                // 如果是圆形碰撞体，计算其世界缩放后的逻辑半径
                if (col is CircleCollider2D circle)
                {
                    float r = circle.radius * Mathf.Max(view.GameObject.transform.lossyScale.x, view.GameObject.transform.lossyScale.y);
                    entity.AddComponent(new CollisionComponent(r));
                }
            }

            // 2. 视觉状态重置 (解决颜色污染的关键)
            if (view.Prefab != null)
            {
                // 获取预制体和当前实例的渲染器 (兼容子物体)
                var prefabSr = view.Prefab.GetComponentInChildren<SpriteRenderer>();
                var instanceSr = view.GameObject.GetComponentInChildren<SpriteRenderer>();

                if (prefabSr != null && instanceSr != null)
                {
                    // 强制同步颜色：让实例变回预制体的初始颜色
                    instanceSr.color = prefabSr.color;
                    // 保存一份基础颜色，供后续受击或减速效果结束后还原
                    entity.AddComponent(new BaseColorComponent(prefabSr.color));
                }
            }

            // 3. 移除标记
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}