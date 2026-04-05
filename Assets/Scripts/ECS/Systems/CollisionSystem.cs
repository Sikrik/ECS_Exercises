using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : SystemBase
{
    public CollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        var pCol = player.GetComponent<CollisionComponent>();
        var pHealth = player.GetComponent<HealthComponent>();
        var config = ECSManager.Instance.Config;

        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, CollisionComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eCol = enemy.GetComponent<CollisionComponent>();

            float dx = ePos.X - pPos.X;
            float dy = ePos.Y - pPos.Y;
            float distSq = dx * dx + dy * dy;
            float radiusSum = pCol.Radius + eCol.Radius;

            if (distSq <= radiusSum * radiusSum)
            {
                // 碰撞伤害逻辑
                if (!player.HasComponent<InvincibleComponent>())
                {
                    var eStats = enemy.GetComponent<EnemyStatsComponent>();
                    if (eStats != null)
                    {
                        pHealth.CurrentHealth -= eStats.Damage;
                        player.AddComponent(new InvincibleComponent { RemainingTime = config.PlayerInvincibleDuration });
                        Debug.Log($"玩家受击！剩余血量: {pHealth.CurrentHealth}");
                    }
                }

                // 碰撞击退逻辑 (仅限 BouncyTag 实体)
                if (enemy.HasComponent<BouncyTag>())
                {
                    float mag = Mathf.Sqrt(distSq);
                    if (mag > 0.01f)
                    {
                        var eStats = enemy.GetComponent<EnemyStatsComponent>();
                        var kb = new KnockbackComponent
                        {
                            DirX = dx / mag,
                            DirY = dy / mag,
                            Timer = GetKnockbackDuration(eStats, config),
                            Speed = GetKnockbackSpeed(eStats, config)
                        };
                        enemy.AddComponent(kb);
                    }
                }

                if (pHealth.CurrentHealth <= 0) break;
            }
        }
    }

    // --- 核心修复：根据新的 GameConfig 字段名进行解析 ---
    private float GetKnockbackDuration(EnemyStatsComponent stats, GameConfig config)
    {
        // 如果没有 Stats 组件，默认使用普通敌人的配置
        if (stats == null) return config.NormalEnemyKnockbackDuration; 
        return stats.Type switch
        {
            EnemyType.Fast => config.FastEnemyKnockbackDuration,
            EnemyType.Tank => config.TankEnemyKnockbackDuration,
            _ => config.NormalEnemyKnockbackDuration // 默认使用 Normal 字段
        };
    }

    private float GetKnockbackSpeed(EnemyStatsComponent stats, GameConfig config)
    {
        if (stats == null) return config.NormalEnemyKnockbackSpeed;
        return stats.Type switch
        {
            EnemyType.Fast => config.FastEnemyKnockbackSpeed,
            EnemyType.Tank => config.TankEnemyKnockbackSpeed,
            _ => config.NormalEnemyKnockbackSpeed
        };
    }
}