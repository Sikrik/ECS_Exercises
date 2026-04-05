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
            var inv = entity.GetComponent<InvincibleComponent>();
            var view = entity.GetComponent<ViewComponent>();

            if (view.GameObject == null || !view.GameObject.TryGetComponent<SpriteRenderer>(out var sr)) continue;

            // 同样的懒加载逻辑
            if (!entity.HasComponent<BaseColorComponent>()) {
                entity.AddComponent(new BaseColorComponent(sr.color));
            }
            var baseColor = entity.GetComponent<BaseColorComponent>().Value;

            inv.RemainingTime -= deltaTime;

            if (inv.RemainingTime <= 0) {
                sr.color = baseColor; // 恢复真正的底色
                entity.RemoveComponent<InvincibleComponent>();
                continue;
            }

            // 闪烁时，RGB 保持 baseColor，只动 Alpha
            float alpha = Mathf.PingPong(Time.time * 12, 1.0f);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }
    }
}