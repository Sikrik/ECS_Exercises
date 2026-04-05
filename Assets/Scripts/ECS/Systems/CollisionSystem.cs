using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞系统：负责处理实体间的物理接触逻辑
/// 重构要点：
/// 1. 使用 InvincibleComponent 替代原本在 Health/Player 组件里的计时器。
/// 2. 只有拥有 BouncyTag 的实体才会产生击退效果。
/// 3. 通过添加 KnockbackComponent 来触发击退，而不是修改基础组件。
/// </summary>
public class CollisionSystem : SystemBase
{
    public CollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 获取玩家实体
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        var pCol = player.GetComponent<CollisionComponent>();
        var pHealth = player.GetComponent<HealthComponent>();
        var config = ECSManager.Instance.Config;

        // 2. 筛选出所有敌人：必须拥有 EnemyTag, PositionComponent 和 CollisionComponent
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent, CollisionComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eCol = enemy.GetComponent<CollisionComponent>();

            // 高性能平方距离碰撞检测
            float dx = ePos.X - pPos.X;
            float dy = ePos.Y - pPos.Y;
            float distSq = dx * dx + dy * dy;
            float radiusSum = pCol.Radius + eCol.Radius;

            if (distSq <= radiusSum * radiusSum)
            {
                // --- 逻辑 A：碰撞伤害 ---
                // 只有当玩家没有 InvincibleComponent 时才触发伤害
                if (!player.HasComponent<InvincibleComponent>())
                {
                    // 获取敌人属性组件读取伤害值
                    var eStats = enemy.GetComponent<EnemyStatsComponent>();
                    if (eStats != null)
                    {
                        pHealth.CurrentHealth -= eStats.Damage;
                        // 给玩家挂载无敌组件
                        player.AddComponent(new InvincibleComponent { RemainingTime = config.PlayerInvincibleDuration });
                        Debug.Log($"玩家受击！剩余血量: {pHealth.CurrentHealth}");
                    }
                }

                // --- 逻辑 B：碰撞弹开 ---
                // 只有拥有 BouncyTag 的敌人发生碰撞才会触发击退
                if (enemy.HasComponent<BouncyTag>())
                {
                    float mag = Mathf.Sqrt(distSq);
                    if (mag > 0.01f)
                    {
                        var eStats = enemy.GetComponent<EnemyStatsComponent>();
                        
                        // 动态挂载击退组件
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

    // 辅助方法：根据敌人类型获取配置的击退参数
    private float GetKnockbackDuration(EnemyStatsComponent stats, GameConfig config)
    {
        if (stats == null) return config.EnemyKnockbackDuration;
        return stats.Type switch
        {
            EnemyType.Fast => config.FastEnemyKnockbackDuration,
            EnemyType.Tank => config.TankEnemyKnockbackDuration,
            _ => config.NormalEnemyKnockbackDuration
        };
    }

    private float GetKnockbackSpeed(EnemyStatsComponent stats, GameConfig config)
    {
        if (stats == null) return config.EnemyKnockbackSpeed;
        return stats.Type switch
        {
            EnemyType.Fast => config.FastEnemyKnockbackSpeed,
            EnemyType.Tank => config.TankEnemyKnockbackSpeed,
            _ => config.NormalEnemyKnockbackSpeed
        };
    }
}