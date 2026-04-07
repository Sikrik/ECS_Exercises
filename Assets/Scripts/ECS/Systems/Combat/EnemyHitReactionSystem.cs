using System.Collections.Generic;

/// <summary>
/// 受击反应系统：负责将“伤害意图”转化为具体的“控制状态”（如硬直）。
/// 重构后：只依赖原子化的 HitRecoveryStatsComponent，不再访问复杂的配置字典。
/// </summary>
public class EnemyHitReactionSystem : SystemBase
{
    public EnemyHitReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 核心逻辑：只抓取本帧受了伤，且拥有硬直配置参数的实体
        var victims = GetEntitiesWith<DamageTakenEventComponent, HitRecoveryStatsComponent>();
        
        foreach (var e in victims)
        {
            var stats = e.GetComponent<HitRecoveryStatsComponent>();
            
            // ==========================================
            // 硬直触发判定
            // ==========================================
            // 1. 检查配置：只有配置了硬直时间大于 0 的单位才会产生反应
            // 2. 状态排他：如果当前正在被“击退”（Knockback），则不重复施加普通硬直，防止表现冲突
            if (stats.Duration > 0 && !e.HasComponent<KnockbackComponent>())
            {
                // 给单位贴上受击硬直标签
                // 随后的 StatusGatherSystem 会因为这个组件将 Speed 设为 0
                // 随后的 HitRecoverySystem 会负责计时并在结束时移除此组件
                if (!e.HasComponent<HitRecoveryComponent>())
                {
                    e.AddComponent(new HitRecoveryComponent { Timer = stats.Duration });
                }
            }
        }
        
        // 列表归还，维持 0 GC
        ReturnListToPool(victims);
    }
}