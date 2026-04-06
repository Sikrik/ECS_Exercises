using UnityEngine;

public static class EnemyFactory {
    public static Entity Create(EnemyType type, Vector3 spawnPos) {
        var ecs = ECSManager.Instance;
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) return null;

        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        Entity enemy = ecs.CreateEntity()
            .AsEnemy()
            .WithBaseView(go, prefab, spawnPos);

        // 统一装载数值组件，实现数据解耦
        enemy.AddComponent(new HealthComponent(recipe.Health));
        enemy.AddComponent(new EnemyStatsComponent { 
            Type = type,
            MoveSpeed = recipe.Speed, 
            Damage = recipe.Damage,
            HitRecoveryDuration = recipe.HitRecoveryDuration // 将配方数值注入组件
        });

        foreach (var trait in recipe.Traits) {
            ComponentRegistry.Apply(enemy, trait);
        }

        return enemy;
    }
}