using System.Collections.Generic;
using UnityEngine;

public class LightningRenderSystem : SystemBase
{
    public LightningRenderSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有闪电组件和视图组件的实体
        var vfxEntities = GetEntitiesWith<LightningVFXComponent, ViewComponent>();
        var ecs = ECSManager.Instance;

        for (int i = vfxEntities.Count - 1; i >= 0; i--)
        {
            var entity = vfxEntities[i];
            var vfx = entity.GetComponent<LightningVFXComponent>();
            var view = entity.GetComponent<ViewComponent>();

            // 1. 初始化 LineRenderer（仅在第一帧执行）
            if (vfx.Line == null && view.GameObject != null)
            {
                vfx.Line = view.GameObject.GetComponent<LineRenderer>();
                if (vfx.Line == null) vfx.Line = view.GameObject.AddComponent<LineRenderer>();
                
                // 设置 LineRenderer 基本属性（也可以在预制体里设好）
                vfx.Line.positionCount = vfx.Segments + 1;
                vfx.Line.startWidth = 0.1f;
                vfx.Line.endWidth = 0.05f;
            }

            // 2. 更新计时器
            vfx.Timer += deltaTime;
            if (vfx.Timer >= vfx.Duration)
            {
                ecs.DestroyEntity(entity);
                continue;
            }

            // 3. 动态生成闪电折线（实现抖动感）
            UpdateLightningVisual(vfx);

            // 4. 实现淡出效果（修改透明度）
            float alpha = 1f - (vfx.Timer / vfx.Duration);
            if (vfx.Line != null)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                );
                vfx.Line.colorGradient = gradient;
            }
        }
    }

    /// <summary>
    /// 在起止点之间计算随机的折点
    /// </summary>
    private void UpdateLightningVisual(LightningVFXComponent vfx)
    {
        if (vfx.Line == null) return;

        Vector3 start = vfx.StartPos;
        Vector3 end = vfx.EndPos;
        vfx.Line.SetPosition(0, start);
        vfx.Line.SetPosition(vfx.Segments, end);

        // 计算闪电方向的法线，用于随机偏移
        Vector3 direction = (end - start).normalized;
        Vector3 normal = new Vector3(-direction.y, direction.x, 0);

        for (int i = 1; i < vfx.Segments; i++)
        {
            float lerpVal = (float)i / vfx.Segments;
            Vector3 midPoint = Vector3.Lerp(start, end, lerpVal);
            
            // 在中间点增加垂直方向的随机位移
            float offset = Random.Range(-vfx.JitterAmount, vfx.JitterAmount);
            vfx.Line.SetPosition(i, midPoint + normal * offset);
        }
    }
}