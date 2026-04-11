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
        
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
        enemy.AddComponent(new FactionComponent(FactionType.Enemy));
        
        // --- 特性装载 ---
        enemy.AddComponent(new NeedsPhysicsBakingTag());
        enemy.AddComponent(new NeedsVisualBakingTag());
        enemy.AddComponent(new MassComponent(recipe.Health)); 

        // 让怪物之间也能互相挤开，形成包围网
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        // ==========================================
        // 👇 【新增：冲锋怪专用能力组装】
        // ==========================================
        // (注：需要确保你的 EnemyType 枚举中包含 Charger，或者配置表的配方 ID 为 Charger)
        if (type.ToString() == "Charger")
        {
            // 赋予冲刺物理能力 (速度提升到 15，持续 0.3 秒，冷却 3 秒)
            enemy.AddComponent(new DashAbilityComponent(12f, 0.3f, 3f));
            
            // 赋予冲锋 AI 决策意图 (距离玩家 6 米以内触发)
            enemy.AddComponent(new ChargerAIComponent(3f));
        }
        // ==========================================

        return enemy;
    }
}