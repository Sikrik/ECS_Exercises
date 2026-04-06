using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无敌视觉系统：处理受击后的闪烁反馈，并确保无敌状态能正常结束。
/// </summary>
public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有处于无敌状态的实体
        var invincibleEntities = GetEntitiesWith<InvincibleComponent>();

        foreach (var entity in invincibleEntities)
        {
            var invincible = entity.GetComponent<InvincibleComponent>();

            // 1. 核心修复：优先处理时间倒计时，确保逻辑闭环
            invincible.RemainingTime -= deltaTime;

            if (invincible.RemainingTime <= 0)
            {
                // 无敌结束，移除组件并尝试恢复视觉
                entity.RemoveComponent<InvincibleComponent>();
                ResetVisual(entity);
                continue;
            }

            // 2. 视觉闪烁逻辑
            if (entity.HasComponent<ViewComponent>())
            {
                var view = entity.GetComponent<ViewComponent>();
                if (view.GameObject != null)
                {
                    // 动态获取渲染器，增加对复杂预制体的兼容性
                    var sr = view.GameObject.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        // 使用 PingPong 产生闪烁效果
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
                    // 确保无敌结束后恢复不透明
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, 1.0f);
                }
            }
        }
    }
}