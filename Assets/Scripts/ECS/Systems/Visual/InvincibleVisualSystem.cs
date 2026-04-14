// 路径: Assets/Scripts/ECS/Systems/Visual/InvincibleVisualSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<InvincibleComponent, ViewComponent>(); 

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var invincible = entity.GetComponent<InvincibleComponent>();
            var view = entity.GetComponent<ViewComponent>();

            invincible.Duration -= deltaTime;

            if (invincible.Duration > 0)
            {
                if (view.SpriteRenderer != null)
                {
                    // 【核心修改】：改成布尔值频闪切换，0.2f 和 1f 形成高落差硬闪烁
                    float alpha = Mathf.PingPong(Time.time * 20f, 1f) > 0.5f ? 1f : 0.15f; 
                    
                    Color currentColor = view.SpriteRenderer.color;
                    currentColor.a = alpha;
                    view.SpriteRenderer.color = currentColor;
                }
            }
            else
            {
                // 无敌结束，恢复不透明状态
                if (view.SpriteRenderer != null)
                {
                    Color currentColor = view.SpriteRenderer.color;
                    currentColor.a = 1f;
                    view.SpriteRenderer.color = currentColor;
                }
                
                entity.RemoveComponent<InvincibleComponent>();
            }
        }
    }
}