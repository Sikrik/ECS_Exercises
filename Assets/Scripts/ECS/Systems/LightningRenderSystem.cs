using System.Collections.Generic;
using UnityEngine;

public class LightningRenderSystem : SystemBase
{
    public LightningRenderSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选视觉组件
        var vfxs = GetEntitiesWith<LightningVFXComponent, ViewComponent>();
        var ecs = ECSManager.Instance;

        for (int i = vfxs.Count - 1; i >= 0; i--)
        {
            var entity = vfxs[i];
            var vfx = entity.GetComponent<LightningVFXComponent>();
            var view = entity.GetComponent<ViewComponent>();

            if (view == null || view.GameObject == null) continue;

            var line = view.GameObject.GetComponent<LineRenderer>();
            if (line == null) continue;

            vfx.Timer += deltaTime;
            if (vfx.Timer >= vfx.Duration) 
            { 
                ecs.DestroyEntity(entity); 
                continue; 
            }

            // 绘制闪电抖动逻辑 (保持不变)
            line.positionCount = 6;
            line.SetPosition(0, vfx.StartPos);
            line.SetPosition(5, vfx.EndPos);
            for (int k = 1; k < 5; k++)
            {
                Vector3 mid = Vector3.Lerp(vfx.StartPos, vfx.EndPos, k / 5f);
                mid += (Vector3)Random.insideUnitCircle * 0.15f;
                line.SetPosition(k, mid);
            }

            // 透明度淡出
            float alpha = 1.0f - (vfx.Timer / vfx.Duration);
            line.startColor = new Color(line.startColor.r, line.startColor.g, line.startColor.b, alpha);
        }
    }
}