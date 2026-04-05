using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var invincibleEntities = GetEntitiesWith<InvincibleComponent, ViewComponent>();

        foreach (var entity in invincibleEntities)
        {
            var invincible = entity.GetComponent<InvincibleComponent>();
            var view = entity.GetComponent<ViewComponent>();

            SpriteRenderer sr = null;
            if (view.GameObject != null) view.GameObject.TryGetComponent(out sr);

            if (sr != null)
            {
                // --- 核心修复：记录原始颜色 ---
                if (invincible.OriginalColor.a == 0)
                {
                    invincible.OriginalColor = sr.color;
                }
            }

            invincible.RemainingTime -= deltaTime;

            if (invincible.RemainingTime <= 0)
            {
                // --- 核心修复：恢复原始颜色 ---
                if (sr != null) sr.color = invincible.OriginalColor;
                
                entity.RemoveComponent<InvincibleComponent>();
                continue;
            }

            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 12, 1.0f);
                // 闪烁时保持原始颜色的 RGB，只改变 Alpha
                sr.color = new Color(invincible.OriginalColor.r, invincible.OriginalColor.g, invincible.OriginalColor.b, alpha);
            }
        }
    }
}