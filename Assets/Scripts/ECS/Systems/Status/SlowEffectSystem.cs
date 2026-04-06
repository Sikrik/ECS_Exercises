using System.Collections.Generic;
using UnityEngine;

public class SlowEffectSystem : SystemBase
{
    // 提取统一的冰蓝色缓存，避免每帧重复 new Color()
    private readonly Color _slowColor = new Color(0.5f, 0.8f, 1f, 1f); 

    public SlowEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取带有减速组件、表现组件和基础颜色组件的实体
        var entities = GetEntitiesWith<SlowEffectComponent, ViewComponent, BaseColorComponent>();

        // 👇 优化点 1：倒序遍历，防止移除组件时引发集合跳位
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var slow = entity.GetComponent<SlowEffectComponent>();
            var view = entity.GetComponent<ViewComponent>();
            var baseColor = entity.GetComponent<BaseColorComponent>();

            // 1. 扣减减速时间
            slow.Duration -= deltaTime;

            // 2. 状态更新与视觉表现
            if (slow.Duration > 0)
            {
                // 👇 优化点 2：直接使用烘焙阶段缓存好的 SpriteRenderer，干掉 GetComponent！
                if (view.SpriteRenderer != null)
                {
                    view.SpriteRenderer.color = _slowColor;
                }
            }
            else
            {
                // 3. 时间结束：恢复原状
                if (view.SpriteRenderer != null)
                {
                    view.SpriteRenderer.color = baseColor.Color;
                }
                
                // 移除减速组件，怪物恢复原本速度，下一帧不再进入此循环
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}