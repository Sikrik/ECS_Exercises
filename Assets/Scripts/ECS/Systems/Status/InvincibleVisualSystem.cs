using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var invincibleEntities = GetEntitiesWith<InvincibleComponent>();

        foreach (var entity in invincibleEntities)
        {
            var invincible = entity.GetComponent<InvincibleComponent>();

            // --- 核心修复 1：必须先进行倒计时，确保无敌状态能正常结束 ---
            invincible.RemainingTime -= deltaTime;
            if (invincible.RemainingTime <= 0)
            {
                entity.RemoveComponent<InvincibleComponent>();
                ResetVisual(entity); // 恢复本色
                continue;
            }

            // 视觉反馈逻辑
            if (entity.HasComponent<ViewComponent>())
            {
                var view = entity.GetComponent<ViewComponent>();
                if (view.GameObject != null)
                {
                    // 使用 GetComponentInChildren 兼容子物体渲染器
                    var sr = view.GameObject.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        float alpha = Mathf.PingPong(Time.time * 15, 1.0f);
                        Color c = sr.color;
                        sr.color = new Color(c.r, c.g, c.b, alpha);
                    }
                }
            }
        }
    }

    private void ResetVisual(Entity entity)
    {
        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                var sr = view.GameObject.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 1.0f);
                }
            }
        }
    }
}