using System.Collections.Generic;

public class RenderSyncSystem : SystemBase
{
    public RenderSyncSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只有这个系统允许操作 SpriteRenderer
        var entities = GetEntitiesWith<ViewComponent, BaseColorComponent>();
        
        foreach (var e in entities)
        {
            var view = e.GetComponent<ViewComponent>();
            var baseColor = e.GetComponent<BaseColorComponent>();
            if (view.SpriteRenderer == null) continue;

            // 根据意图组件决定最终颜色
            if (e.HasComponent<ColorTintComponent>())
            {
                view.SpriteRenderer.color = e.GetComponent<ColorTintComponent>().TargetColor;
            }
            else
            {
                view.SpriteRenderer.color = baseColor.Color;
            }
        }
    }
}