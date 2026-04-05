using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无敌视觉与状态系统：
/// 1. 负责处理无敌期间的闪烁反馈。
/// 2. 负责无敌时间的计时，并在结束时移除无敌状态。
/// </summary>
public class InvincibleVisualSystem : SystemBase
{
    public InvincibleVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 获取所有正处于无敌状态的实体
        var invincibleEntities = GetEntitiesWith<InvincibleComponent, ViewComponent>();

        foreach (var entity in invincibleEntities)
        {
            var invincible = entity.GetComponent<InvincibleComponent>();
            var view = entity.GetComponent<ViewComponent>();

            // --- 核心修复：更新计时器 ---
            invincible.RemainingTime -= deltaTime;

            // --- 状态移除：如果时间到期，移除无敌组件 ---
            if (invincible.RemainingTime <= 0)
            {
                entity.RemoveComponent<InvincibleComponent>();
                
                // 视觉恢复：确保移除时颜色恢复正常
                if (view.GameObject != null && view.GameObject.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.color = Color.white;
                }
                continue; // 跳过后续视觉处理
            }

            // --- 视觉反馈：无敌期间持续闪烁 ---
            if (view.GameObject != null && view.GameObject.TryGetComponent<SpriteRenderer>(out var sr_blink))
            {
                // 使用 PingPong 实现高频闪烁效果
                float alpha = Mathf.PingPong(Time.time * 12, 1.0f);
                sr_blink.color = new Color(1, 1, 1, alpha);
            }
        }
    }
}