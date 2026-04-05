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

            if (sr != null) {
                // --- 核心修复：初次记录原始颜色 ---
                if (slow.OriginalColor.a == 0) slow.OriginalColor = sr.color;
                sr.color = IceBlue;
            }

            slow.RemainingDuration -= deltaTime;

            if (slow.RemainingDuration <= 0) {
                // --- 核心修复：恢复原始颜色 ---
                if (sr != null) sr.color = slow.OriginalColor;

                // 清理 Attached VFX
                if (entity.HasComponent<AttachedVFXComponent>()) {
                    var vfx = entity.GetComponent<AttachedVFXComponent>();
                    // 这里由于不是 Entity 销毁，手动调一次池子回收
                    if (vfx.EffectObject != null)
                        PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfx.EffectObject);
                    entity.RemoveComponent<AttachedVFXComponent>();
                }
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}