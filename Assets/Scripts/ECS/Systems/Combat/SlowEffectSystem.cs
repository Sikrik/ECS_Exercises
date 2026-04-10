using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 减速逻辑系统（纯逻辑层）
/// 职责：仅处理减速时间的倒计时，并对外下发“变色意图”
/// </summary>
public class SlowEffectSystem : SystemBase
{
    // 冰蓝色意图
    private readonly Color _slowColor = new Color(0.5f, 0.8f, 1f, 1f); 

    public SlowEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 纯逻辑查询：只关心拥有减速组件的实体，完全不碰 ViewComponent
        var entities = GetEntitiesWith<SlowEffectComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var slow = entity.GetComponent<SlowEffectComponent>();

            // 1. 扣减减速时间
            slow.Duration -= deltaTime;

            if (slow.Duration > 0)
            {
                // 2. 状态生效中：下发视觉变色意图
                if (!entity.HasComponent<ColorTintComponent>())
                {
                    entity.AddComponent(new ColorTintComponent(_slowColor));
                }
            }
            else
            {
                // 3. 减速结束：撤销变色意图
                entity.RemoveComponent<ColorTintComponent>();

                // 处理特效销毁
                if (entity.HasComponent<AttachedVFXComponent>())
                {
                    var vfx = entity.GetComponent<AttachedVFXComponent>();
                    if (vfx.EffectObject != null)
                    {
                        UnityEngine.Object.Destroy(vfx.EffectObject); 
                    }
                    entity.RemoveComponent<AttachedVFXComponent>(); 
                }
                
                // 移除减速组件，恢复正常状态
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}