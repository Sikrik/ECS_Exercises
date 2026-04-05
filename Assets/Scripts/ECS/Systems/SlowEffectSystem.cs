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

            if (view.GameObject == null) continue;
            if (!view.GameObject.TryGetComponent<SpriteRenderer>(out var sr)) continue;

            // --- 核心修复：确保记录的是真正的基础颜色 ---
            if (!entity.HasComponent<BaseColorComponent>()) {
                entity.AddComponent(new BaseColorComponent(sr.color));
            }
            var baseColor = entity.GetComponent<BaseColorComponent>().Value;

            // 持续变蓝
            sr.color = IceBlue;
            slow.RemainingDuration -= deltaTime;

            if (slow.RemainingDuration <= 0) {
                // 恢复基础颜色，而不是写死 Color.white 或 slow.OriginalColor
                sr.color = baseColor;
            
                // 清理 VFX 逻辑保持不变...
                if (entity.HasComponent<AttachedVFXComponent>()) {
                    var vfx = entity.GetComponent<AttachedVFXComponent>();
                    if (vfx.EffectObject != null)
                        PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfx.EffectObject);
                    entity.RemoveComponent<AttachedVFXComponent>();
                }
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}