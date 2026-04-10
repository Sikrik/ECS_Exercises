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
        enemy.AddComponent(new SpeedComponent(recipe.Speed)); 
        enemy.AddComponent(new HealthComponent(recipe.Health)); 
        enemy.AddComponent(new DamageComponent(recipe.Damage)); 

        // --- 原子化配置组件 ---
        enemy.AddComponent(new BountyComponent(recipe.EnemyDeathScore)); 
        enemy.AddComponent(new HitRecoveryStatsComponent(recipe.HitRecoveryDuration));
        enemy.AddComponent(new BounceForceComponent(recipe.BounceForce));
        
        // 👇 【核心重构：赋予肉身冲撞反馈设定】
        // 方案二：怪物碰怪物、怪物碰玩家，只产生物理弹性排斥 (Bounce)，不会导致对方陷入受击硬直 (Recovery)
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));

        // --- 特性装载 ---
        enemy.AddComponent(new NeedsBakingTag());
        enemy.AddComponent(new MassComponent(recipe.Health)); 

        // 让怪物之间也能互相挤开，形成包围网
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        return enemy;
    }
}