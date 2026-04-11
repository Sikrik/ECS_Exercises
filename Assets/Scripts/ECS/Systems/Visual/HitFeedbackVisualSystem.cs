using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 受击表现系统（表现层）
/// 职责：监听实体的硬直状态，实现高频红白闪烁的打击感表现
/// </summary>
public class HitFeedbackVisualSystem : SystemBase
{
    public HitFeedbackVisualSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选正在受击硬直中，且具有视觉组件的实体
        var entities = GetEntitiesWith<HitRecoveryComponent, ViewComponent>();

        foreach (var entity in entities)
        {
            var view = entity.GetComponent<ViewComponent>();
            
            // 状态维持与表现：高频白红闪烁
            if (view.SpriteRenderer != null)
            {
                float lerp = Mathf.PingPong(Time.time * 10f, 1f);
                view.SpriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f), lerp);
            }
        }
        ReturnListToPool(entities);
    }
}