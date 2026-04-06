using UnityEngine;

public static class EnemyFactory
{
    public static Entity Create(EnemyType type, Vector3 spawnPos)
    {
        var ecs = ECSManager.Instance;
        // 1. 获取配方 (根据枚举名匹配 CSV ID)
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) return null;

        // 2. 表现层实例化
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 3. 逻辑流水线组装 (严格按你要求的顺序)
        Entity enemy = ecs.CreateEntity()
            .AsEnemy() // 第一步：打身份 Tag
            .WithBaseView(go, prefab, spawnPos); // 第二步：装基础组件

        // 注入配方数值
        enemy.AddComponent(new HealthComponent(recipe.Health));
        enemy.AddComponent(new EnemyStatsComponent { MoveSpeed = recipe.Speed, Damage = recipe.Damage });

        // 第三步：动态装配进阶组件 (Data-Driven Traits)
        foreach (var trait in recipe.Traits)
        {
            ComponentRegistry.Apply(enemy, trait);
        }

        return enemy;
    }
}