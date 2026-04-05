using System.Collections.Generic;
using UnityEngine;

public class EnemyAISystem : SystemBase
{
    public EnemyAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        if (Time.timeScale <= 0) return;

        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count == 0) return;
        var pPos = players[0].GetComponent<PositionComponent>();

        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, VelocityComponent, EnemyStatsComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eVel = enemy.GetComponent<VelocityComponent>();
            var eStats = enemy.GetComponent<EnemyStatsComponent>();

            if (enemy.HasComponent<KnockbackComponent>())
            {
                UpdateKnockbackState(enemy, eVel, deltaTime);
                continue;
            }

            if (enemy.HasComponent<HitRecoveryComponent>())
            {
                UpdateRecoveryState(enemy, eVel, ePos, pPos, eStats, deltaTime);
                continue;
            }

            UpdateTrackingState(enemy, eVel, ePos, pPos, eStats);
        }
    }

    private void UpdateKnockbackState(Entity entity, VelocityComponent vel, float dt)
    {
        var kb = entity.GetComponent<KnockbackComponent>();
        float progress = kb.Timer / 0.2f; 
        vel.X = kb.DirX * kb.Speed * progress;
        vel.Y = kb.DirY * kb.Speed * progress;

        kb.Timer -= dt;
        if (kb.Timer <= 0) entity.RemoveComponent<KnockbackComponent>();
    }

    private void UpdateRecoveryState(Entity entity, VelocityComponent vel, PositionComponent ePos, PositionComponent pPos, EnemyStatsComponent stats, float dt)
    {
        var recovery = entity.GetComponent<HitRecoveryComponent>();
        float dx = pPos.X - ePos.X; float dy = pPos.Y - ePos.Y;
        float mag = Mathf.Sqrt(dx * dx + dy * dy);

        if (mag > 0.1f)
        {
            float progress = 1.0f - (recovery.Timer / 0.4f); 
            vel.X = (dx / mag) * stats.MoveSpeed * progress;
            vel.Y = (dy / mag) * stats.MoveSpeed * progress;
        }

        recovery.Timer -= dt;
        if (recovery.Timer <= 0) entity.RemoveComponent<HitRecoveryComponent>();
    }

    private void UpdateTrackingState(Entity enemy, VelocityComponent vel, PositionComponent ePos, PositionComponent pPos, EnemyStatsComponent stats)
    {
        float dx = pPos.X - ePos.X; float dy = pPos.Y - ePos.Y;
        float mag = Mathf.Sqrt(dx * dx + dy * dy);

        if (mag > 0.1f)
        {
            float finalSpeed = stats.MoveSpeed;
            if (enemy.HasComponent<SlowEffectComponent>())
            {
                finalSpeed *= (1.0f - enemy.GetComponent<SlowEffectComponent>().SlowRatio);
            }
            vel.X = (dx / mag) * finalSpeed;
            vel.Y = (dy / mag) * finalSpeed;
        }
        else
        {
            vel.X = 0; vel.Y = 0;
        }
    }
}