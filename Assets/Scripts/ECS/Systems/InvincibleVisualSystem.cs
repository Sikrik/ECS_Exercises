using System.Collections.Generic;
using UnityEngine;

public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 处理正在无敌的实体（闪烁）
        var invincibleEntities = GetEntitiesWith<InvincibleComponent, ViewComponent>();
        foreach (var entity in invincibleEntities)
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null && view.GameObject.TryGetComponent<SpriteRenderer>(out var sr))
            {
                // 使用 PingPong 实现高频闪烁
                float alpha = Mathf.PingPong(Time.time * 12, 1.0f);
                sr.color = new Color(1, 1, 1, alpha);
            }
        }

        // 2. 恢复非无敌玩家的状态（确保无敌结束后恢复不透明）
        var players = GetEntitiesWith<PlayerTag, ViewComponent>();
        foreach (var p in players)
        {
            if (!p.HasComponent<InvincibleComponent>())
            {
                var view = p.GetComponent<ViewComponent>();
                if (view.GameObject != null && view.GameObject.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    if (sr.color.a < 1.0f) sr.color = Color.white;
                }
            }
        }
    }
}