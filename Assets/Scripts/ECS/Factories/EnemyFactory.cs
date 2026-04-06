// 重构后的 EnemyFactory.cs

using UnityEngine;

public static class EnemyFactory
{
    public static Entity Create(EnemyType type, Vector3 spawnPos)
    {
        var ecs = ECSManager.Instance;
        var config = ecs.Config; //
        
        // 1. 表现层准备 (从对象池获取)
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type); //
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity); //

        // 2. 逻辑层流水线组装：先打 Tag -> 装基础 -> 装进阶 
        Entity enemy = ecs.CreateEntity()
            .AsEnemy() // 身份识别
            .WithBaseView(go, prefab, spawnPos) // 基础物理与视觉
            .ApplyTypeLogic(type, config); // 应用特定类型的数值逻辑

        return enemy;
    }

    // 辅助方法：将配置表数值映射到流水线上
    private static Entity ApplyTypeLogic(this Entity entity, EnemyType type, GameConfig config)
    {
        return type switch
        {
            EnemyType.Fast => entity
                .WithCombatStats(config.FastEnemyMaxHealth, config.FastEnemySpeed, config.EnemyDamage)
                .WithBouncy(), // 快速怪带反弹
            
            EnemyType.Tank => entity
                .WithCombatStats(config.TankEnemyMaxHealth, config.TankEnemySpeed, config.EnemyDamage), // 坦克不反弹
            
            _ => entity
                .WithCombatStats(config.EnemyMaxHealth, config.EnemyMoveSpeed, config.EnemyDamage)
                .WithBouncy(),
        };
    }
}