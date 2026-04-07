using System.Collections.Generic;

public class EnemyHitReactionSystem : SystemBase
{
    public EnemyHitReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只抓取本帧受了伤的怪物
        var enemies = GetEntitiesWith<EnemyTag, DamageTakenEventComponent, EnemyStatsComponent>();
        
        foreach (var e in enemies)
        {
            var stats = e.GetComponent<EnemyStatsComponent>();
            // 如果配方里配置了硬直时间，且怪物当前没有被击退
            if (stats.Config.HitRecoveryDuration > 0 && !e.HasComponent<KnockbackComponent>())
            {
                // 给怪物贴上真正的硬直标签，交给后面的 HitRecoverySystem 去处理闪烁和计时
                if (!e.HasComponent<HitRecoveryComponent>())
                {
                    e.AddComponent(new HitRecoveryComponent { Timer = stats.Config.HitRecoveryDuration });
                }
            }
        }
    }
}