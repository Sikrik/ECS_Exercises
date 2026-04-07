using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) return null;

        GameObject prefab = GameObject_PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        Entity enemy = ecs.CreateEntity();
    
        // --- 基础组件 ---
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new SpeedComponent(recipe.Speed)); //
        enemy.AddComponent(new HealthComponent(recipe.Health)); //
        enemy.AddComponent(new DamageComponent(recipe.Damage)); //

        // --- 【重构重点】原子化配置组件 ---
        // 以后系统直接查 BountyComponent，不需要经过 EnemyStats
        enemy.AddComponent(new BountyComponent(recipe.EnemyDeathScore)); 
        enemy.AddComponent(new HitRecoveryStatsComponent(recipe.HitRecoveryDuration));

        // --- 特性装载 ---
        enemy.AddComponent(new NeedsBakingTag());
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        return enemy;
    }
}