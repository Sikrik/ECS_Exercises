using System.Collections.Generic;

/// <summary>
/// 受击硬直系统（纯逻辑层）
/// 职责：仅负责扣减硬直时间，并在结束时移除状态组件
/// </summary>
public class HitRecoverySystem : SystemBase
{
    public HitRecoverySystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 纯逻辑查询，不再获取 ViewComponent 和 BaseColorComponent
        var entities = GetEntitiesWith<HitRecoveryComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var recovery = entity.GetComponent<HitRecoveryComponent>();

            // 1. 扣减硬直时间
            recovery.Timer -= deltaTime;

            // 2. 状态结束，移除组件。
            // (无需手动恢复颜色，表现层管线的 RenderSyncSystem 会自动接管)
            if (recovery.Timer <= 0)
            {
                entity.RemoveComponent<HitRecoveryComponent>();
            }
        }
        ReturnListToPool(entities);
    }
}