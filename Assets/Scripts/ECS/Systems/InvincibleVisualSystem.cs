using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无敌视觉系统：处理受击后的闪烁反馈并恢复本色
/// </summary>
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

            // 安全获取 SpriteRenderer
            SpriteRenderer sr = null;
            if (view.GameObject != null)
            {
                view.GameObject.TryGetComponent(out sr);
            }

            if (sr == null) continue;

            // --- 核心修复：记录基础颜色 (懒加载) ---
            if (!entity.HasComponent<BaseColorComponent>())
            {
                entity.AddComponent(new BaseColorComponent(sr.color));
            }
            var baseColor = entity.GetComponent<BaseColorComponent>().Value;

            invincible.RemainingTime -= deltaTime;

            // 无敌结束逻辑
            if (invincible.RemainingTime <= 0)
            {
                entity.RemoveComponent<InvincibleComponent>();
                
                // 恢复到基础颜色
                sr.color = baseColor;
                continue;
            }

            // --- 闪烁反馈逻辑 ---
            // 使用 PingPong 改变 Alpha，但保持 BaseColor 的 RGB 值
            float alpha = Mathf.PingPong(Time.time * 12, 1.0f);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }
    }
}