using System.Collections.Generic;

/// <summary>
/// 受击反应系统：负责将“伤害意图”转化为具体的“控制状态”（如硬直）。
/// </summary>
public class EnemyHitReactionSystem : SystemBase
{
    public EnemyHitReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取本帧受了伤，且拥有硬直配置参数的实体
        var victims = GetEntitiesWith<DamageTakenEventComponent, HitRecoveryStatsComponent>();
        
        foreach (var e in victims)
        {
            var stats = e.GetComponent<HitRecoveryStatsComponent>();
            var dmgEvt = e.GetComponent<DamageTakenEventComponent>();
            
            // ==========================================
            // 【核心修改】硬直触发判定
            // ==========================================
            // 1. 该伤害事件明确允许造成硬直 (子弹为 false，肉体冲撞为 true)
            // 2. 检查配置：硬直时间必须大于 0
            // 3. 状态排他：当前不能正在被“击退”
            if (dmgEvt.CauseHitRecovery && stats.Duration > 0 && !e.HasComponent<KnockbackComponent>())
            {
                if (!e.HasComponent<HitRecoveryComponent>())
                {
                    e.AddComponent(new HitRecoveryComponent { Timer = stats.Duration });
                }
            }
        }
        
        ReturnListToPool(victims);
    }
}