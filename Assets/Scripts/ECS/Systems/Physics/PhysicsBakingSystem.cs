using System.Collections.Generic;
using UnityEngine;

public class PhysicsBakingSystem : SystemBase
{
    public PhysicsBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出需要烘焙的实体
        var pending = GetEntitiesWith<NeedsBakingTag, ViewComponent>();
        // 优化点 1：倒序遍历，防止在循环中调用 RemoveComponent 导致集合修改引发报错或跳位
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
                
                // 【核心修复 2】：兼容多种碰撞体类型，确保都能生成逻辑碰撞半径
                float maxScale = Mathf.Max(view.GameObject.transform.lossyScale.x, view.GameObject.transform.lossyScale.y);
                
                if (col is CircleCollider2D circle)
                {
                    float r = circle.radius * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
                else if (col is BoxCollider2D box)
                {
                    // 近似处理：取 Box 长宽的最大值的一半作为逻辑排斥半径
                    float r = Mathf.Max(box.size.x, box.size.y) * 0.5f * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
                else if (col is CapsuleCollider2D capsule)
                {
                    // 近似处理：取胶囊体长宽的最大值的一半作为逻辑排斥半径
                    float r = Mathf.Max(capsule.size.x, capsule.size.y) * 0.5f * maxScale;
                    entity.AddComponent(new CollisionComponent(r));
                }
            }

            // ==========================================
            // 优化点 2：视觉状态缓存与重置 (消灭 GetComponent 的核心)
            // ==========================================
            // 在烘焙阶段（实体刚从池子里生成出来），直接把渲染器缓存进 ViewComponent
            if (view.SpriteRenderer == null)
            {
                view.SpriteRenderer = view.GameObject.GetComponentInChildren<SpriteRenderer>();
            }
            if (view.SpriteRenderer != null)
            {
                view.SpriteRenderer.enabled = true; 
            }
            if (view.Prefab != null && view.SpriteRenderer != null)
            {
                // 获取预制体的渲染器
                var prefabSr = view.Prefab.GetComponentInChildren<SpriteRenderer>();

                if (prefabSr != null)
                {
                    // 强制同步颜色：直接使用缓存的渲染器，让实例变回预制体的初始颜色
                    view.SpriteRenderer.color = prefabSr.color;
                    
                    // 保存一份基础颜色，供后续受击或减速效果结束后还原
                    entity.AddComponent(new BaseColorComponent(prefabSr.color));
                }
            }
            // 3. 烘焙完成，移除标记，使其不再进入这个循环
            entity.RemoveComponent<NeedsBakingTag>();
        }
    }
}