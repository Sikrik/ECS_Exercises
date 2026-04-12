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
            // 硬直触发判定
            // ==========================================
            // 1. 该伤害事件明确允许造成硬直 (子弹为 false，有附魔时为 true，肉体冲撞为 true)
            // 2. 状态排他：当前不能正在被“击退”
            if (dmgEvt.CauseHitRecovery && !e.HasComponent<KnockbackComponent>())
            {
                // 【核心修改】：优先使用伤害事件传来的覆盖时间(由武器升级决定)，如果没有，才使用怪物自身的基础硬直配置
                float finalDuration = dmgEvt.RecoveryDurationOverride > 0 ? dmgEvt.RecoveryDurationOverride : stats.Duration;
                
                if (finalDuration > 0 && !e.HasComponent<HitRecoveryComponent>())
                {
                    e.AddComponent(new HitRecoveryComponent { Timer = finalDuration });
                }
            }
        }
    }
}