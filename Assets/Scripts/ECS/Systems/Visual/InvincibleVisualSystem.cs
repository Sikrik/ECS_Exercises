using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取所有处于无敌状态的实体
        var entities = GetEntitiesWith<InvincibleComponent, ViewComponent, BaseColorComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var invincible = entity.GetComponent<InvincibleComponent>();
            var view = entity.GetComponent<ViewComponent>();
            var baseColor = entity.GetComponent<BaseColorComponent>();

            // 1. 扣减无敌时间
            invincible.Duration -= deltaTime;

            if (invincible.Duration> 0)
            {
                // 2. 播放无敌闪烁特效 (通过 Time.time 实现平滑透明度渐变)
                if (view.SpriteRenderer != null)
                {
                    // 使用 Mathf.PingPong 让 alpha 在 0.5 到 1 之间快速循环
                    float alpha = Mathf.PingPong(Time.time * 15f, 1f) * 0.5f + 0.5f; 
                    
                    // 基于基础颜色创建闪烁颜色，只改变透明度
                    Color flashColor = baseColor.Color;
                    flashColor.a = alpha;
                    
                    // 👇 直接赋值给缓存的渲染器
                    view.SpriteRenderer.color = flashColor;
                }
            }
            else
            {
                // 3. 无敌结束，恢复基础颜色 (完全不透明)
                if (view.SpriteRenderer != null)
                {
                    view.SpriteRenderer.color = baseColor.Color;
                }
                
                // 移除无敌组件，让实体重新变得可受击
                entity.RemoveComponent<InvincibleComponent>();
            }
        }
    }
}