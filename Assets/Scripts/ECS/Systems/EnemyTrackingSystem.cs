using System.Collections.Generic;
using UnityEngine;

public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        // 核心解耦点：通过 GetEntitiesWith 自动过滤掉处于“击退”或“硬直”状态的敌人
        // 只要在筛选条件里不包含 KnockbackComponent，处于击退中的怪就不会进这个循环
        var trackingEnemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent>();

        foreach (var enemy in trackingEnemies)
        {
            // 如果身上有击退或硬直，跳过追踪（意图被抑制）
            if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>()) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var vel = enemy.GetComponent<VelocityComponent>();
            var stats = enemy.GetComponent<EnemyStatsComponent>();

            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist > 0.1f)
            {
                float speed = stats.MoveSpeed;
                if (enemy.HasComponent<SlowEffectComponent>())
                    speed *= (1f - enemy.GetComponent<SlowEffectComponent>().SlowRatio);

                vel.VX = (dx / dist) * speed;
                vel.VY = (dy / dist) * speed;
            }
        }
    }
}