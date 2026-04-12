using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<InvincibleComponent, ViewComponent>(); // 不再需要 BaseColorComponent

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
                    float alpha = Mathf.PingPong(Time.time * 15f, 1f) * 0.5f + 0.5f; 
                    
                    // 【核心修改】：读取当前的 RGB（可能是减速的蓝或受击的红），只修改它的透明度 Alpha
                    Color currentColor = view.SpriteRenderer.color;
                    currentColor.a = alpha;
                    view.SpriteRenderer.color = currentColor;
                }
            }
            else
            {
                // 无敌结束，恢复不透明状态 (RGB 依然保留当前状态应有的颜色)
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