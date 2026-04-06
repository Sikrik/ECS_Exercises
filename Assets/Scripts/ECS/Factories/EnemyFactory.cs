using UnityEngine;

public static class EnemyFactory
{
    public static Entity Create(EnemyType type, Vector3 spawnPos)
    {
        var ecs = ECSManager.Instance;
        // 1. 将枚举转为字符串 ID 从字典拿配方
        string enemyId = type.ToString(); 
        if (!ecs.Config.EnemyRecipes.TryGetValue(enemyId, out var data)) return null;

        // 2. 表现层准备
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 3. 通用流水线组装
        Entity enemy = ecs.CreateEntity()
            .AsEnemy()
            .WithBaseView(go, prefab, spawnPos);

        // 装载基础数值
        enemy.AddComponent(new HealthComponent(data.Health));
        enemy.AddComponent(new EnemyStatsComponent { 
            MoveSpeed = data.Speed, 
            Damage = data.Damage 
        });

        // 4. 核心解耦：动态特性装配
        foreach (var trait in data.Traits)
        {
            ComponentRegistry.Apply(enemy, trait);
        }

        return enemy;
    }
}