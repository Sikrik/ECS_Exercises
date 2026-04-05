using System.Collections.Generic;
using UnityEngine;

public class SlowEffectSystem : SystemBase
{
    private readonly Color IceBlue = new Color(0.4f, 0.7f, 1.0f, 1.0f);

    public SlowEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<SlowEffectComponent, ViewComponent>();

        foreach (var entity in entities)
        {
            var slow = entity.GetComponent<SlowEffectComponent>();
            var view = entity.GetComponent<ViewComponent>();

            SpriteRenderer sr = null;
            if (view.GameObject != null) view.GameObject.TryGetComponent(out sr);

            if (sr != null)
            {
                // --- 核心修复：如果是第一次执行，记录原始颜色 ---
                if (slow.OriginalColor.a == 0) // 通过 alpha 为 0 判断是否是初次记录
                {
                    slow.OriginalColor = sr.color;
                }
                
                sr.color = IceBlue;
            }

            slow.RemainingDuration -= deltaTime;

            if (slow.RemainingDuration <= 0)
            {
                // --- 核心修复：恢复记录的原始颜色 ---
                if (sr != null) sr.color = slow.OriginalColor;

                if (entity.HasComponent<AttachedVFXComponent>())
                {
                    var vfxComp = entity.GetComponent<AttachedVFXComponent>();
                    if (vfxComp.EffectObject != null) PoolManager.Instance.Despawn(vfxComp.EffectObject);
                    entity.RemoveComponent<AttachedVFXComponent>();
                }

                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}