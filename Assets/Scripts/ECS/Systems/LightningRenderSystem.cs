using System.Collections.Generic;
using UnityEngine;

public class LightningRenderSystem : SystemBase
{
    public LightningRenderSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var vfxs = GetEntitiesWith<LightningVFXComponent, ViewComponent>();

        for (int i = vfxs.Count - 1; i >= 0; i--)
        {
            var entity = vfxs[i];
            var vfx = entity.GetComponent<LightningVFXComponent>();
            var line = entity.GetComponent<ViewComponent>().GameObject.GetComponent<LineRenderer>();

            vfx.Timer += deltaTime;
            if (vfx.Timer >= vfx.Duration)
            {
                ECSManager.Instance.DestroyEntity(entity);
                continue;
            }

            // 绘制逻辑（抖动效果）
            line.positionCount = 6;
            line.SetPosition(0, vfx.StartPos);
            line.SetPosition(5, vfx.EndPos);
            for(int k=1; k<5; k++) {
                Vector3 mid = Vector3.Lerp(vfx.StartPos, vfx.EndPos, k/5f);
                mid += (Vector3)Random.insideUnitCircle * 0.2f;
                line.SetPosition(k, mid);
            }
        }
    }
}