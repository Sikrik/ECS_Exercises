using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        
        // 1. 获取配置配方 (EnemyData)
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) 
        {
            Debug.LogError($"未找到敌人配置: {recipeId}");
            return null;
        }

        // 2. 表现层：从对象池获取
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 3. 逻辑层：创建实体并进行基础装配
        Entity enemy = ecs.CreateEntity();
        
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new NeedsBakingTag());
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));

        // 4. 数值装载：直接引用配置对象
        enemy.AddComponent(new HealthComponent(recipe.Health));
        enemy.AddComponent(new DamageComponent(recipe.Damage));
        enemy.AddComponent(new StatusSummaryComponent()); 

        // 【重构核心】：不再拷贝 Speed, Score 等，直接存入 recipe 引用
        enemy.AddComponent(new EnemyStatsComponent 
        { 
            Type = type,
            Config = recipe, // 直接持有配置引用
            CurrentMoveSpeed = recipe.Speed // 仅初始化当前动态速度
        });

        // 5. 特性装载 (Traits)
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
            {
                if (trait == "Bouncy") enemy.AddComponent(new BouncyTag());
                else ComponentRegistry.Apply(enemy, trait);
            }
        }
        return enemy;
    }
}