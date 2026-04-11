using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视觉烘焙系统 (属于 Presentation 表现组)
/// 职责：仅在实体生成的第一帧执行，缓存 SpriteRenderer 并记录初始颜色，消灭后续所有的 GetComponent 开销。
/// </summary>
public class VisualBakingSystem : SystemBase
{
    public VisualBakingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 查询视觉烘焙标签
        var pending = GetEntitiesWith<NeedsVisualBakingTag, ViewComponent>();
        
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            var entity = pending[i];
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject == null) continue;

            // 1. 缓存 SpriteRenderer
            if (view.SpriteRenderer == null)
            {
                view.SpriteRenderer = view.GameObject.GetComponentInChildren<SpriteRenderer>();
            }
            
            // 2. 初始化视觉状态
            if (view.SpriteRenderer != null)
            {
                view.SpriteRenderer.enabled = true; 
                
                if (view.Prefab != null)
                {
                    // 获取预制体的渲染器
                    var prefabSr = view.Prefab.GetComponentInChildren<SpriteRenderer>();
                    if (prefabSr != null)
                    {
                        // 强制同步颜色：让实例变回预制体的初始颜色
                        view.SpriteRenderer.color = prefabSr.color;
                        
                        // 保存一份基础颜色，供后续受击或减速效果结束后还原
                        entity.AddComponent(new BaseColorComponent(prefabSr.color));
                    }
                }
            }

            // 3. 视觉烘焙完成，移除标签
            entity.RemoveComponent<NeedsVisualBakingTag>();
        }
    }
}